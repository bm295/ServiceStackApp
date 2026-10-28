# Trợ lý tạo bản nháp trước buổi khám

Chỉ sử dụng hồ sơ giả lập. Backend thực thi chain có nhánh:

```text
Snapshot hồ sơ → generator → deserialize/schema → kiểm tra nguồn
                         ↑ lỗi JSON/schema/timeout: thử lại 1 lần
                         └ hết lượt: Failed
Nguồn sai bệnh nhân/không tồn tại/sai phiên bản → NeedsManualReview
Hợp lệ → PendingReview → bác sĩ sửa, kiểm tra lại nguồn → Approved
```

Mỗi mục trong cả ba phần có `Sources` gồm `SourceId` và `Version`. Mục dữ liệu thiếu trích dẫn ghi chép cho thấy thông tin chưa được ghi nhận. Validator không kiểm chứng ý nghĩa lâm sàng; bác sĩ chịu trách nhiệm đọc nguồn và duyệt. Nội dung có nguồn sai không được lưu thành bản nháp có thể duyệt; snapshot và lý do từ chối vẫn được lưu.

## Chạy demo qua API

Từ thư mục gốc, cấu hình secret riêng cho một bác sĩ (không commit secret):

```powershell
$env:VisitSummaries__Doctors__doctor1 = 'local-demo-secret'
dotnet run --project src/WolverinePlayground --urls http://localhost:5080
```

Terminal khác:

```powershell
$headers = @{ 'X-Doctor-Key' = 'local-demo-secret' }
$base = 'http://localhost:5080/visit-summaries'
Invoke-RestMethod "$base/visits" -Headers $headers
$draft = Invoke-RestMethod "$base/visits/visit-1" -Headers $headers -Method Post -ContentType application/json -Body (@{ requestId = [guid]::NewGuid().ToString(); scenario = 'valid' } | ConvertTo-Json)
# Bác sĩ có thể sửa text trong $draft.content, sau khi đọc nguồn ở $draft.snapshot.sources.
$approval = @{ expectedRevision = $draft.revision; content = $draft.content } | ConvertTo-Json -Depth 20
Invoke-RestMethod "$base/$($draft.id)/approve" -Headers $headers -Method Post -ContentType application/json -Body $approval

foreach ($scenario in @('foreign-source', 'timeout', 'invalid-json', 'recover')) {
    $body = @{ requestId = [guid]::NewGuid().ToString(); scenario = $scenario } | ConvertTo-Json
    $result = Invoke-RestMethod "$base/visits/visit-1" -Headers $headers -Method Post -ContentType application/json -Body $body
    $result | Select-Object id, status, history
}
```

`foreign-source` → `NeedsManualReview` sau một lần. `timeout` và `invalid-json` → `Failed` sau hai lần. `recover` lỗi JSON lần đầu rồi `PendingReview`. Các lỗi mô phỏng trả ngay, không cần đợi timeout thật. Dùng lại `requestId` nhận cùng bản ghi, không gọi lại generator; dùng requestId mới để tạo lần thực thi mới. Khóa đã gắn với lượt khám khác trả 409. GET `/{id}` đọc trạng thái và lịch sử. Duyệt revision cũ hoặc bản nháp không pending trả 409; sửa nguồn sai trả 400 và ghi `ApprovalRejected`.

## Provider HTTP

Mặc định dùng fake generator để demo không cần tài khoản. Chuyển sang một provider có API chat-completions tương thích bằng biến môi trường:

```powershell
$env:VisitSummaries__Provider = 'Http'
$env:VisitSummaries__LlmEndpoint = 'https://YOUR-PROVIDER/v1/chat/completions'
$env:VisitSummaries__LlmModel = 'YOUR-MODEL'
$env:VisitSummaries__LlmApiKey = 'YOUR-SECRET'
```

Chế độ HTTP chỉ nhận scenario `valid`. Mỗi lần gọi giới hạn 20 giây; tối đa hai lần. Không gửi tên, ngày sinh hay thông tin liên hệ. Chưa kiểm thử tích hợp với dịch vụ LLM thật; các test dùng fake và database thật.

## Lưu trữ và giới hạn demo

SQLite mặc định tại `src/WolverinePlayground/visit-summaries.db`; có thể đổi `ConnectionStrings__VisitSummaries`. Payload gồm snapshot nguồn, nội dung, trạng thái, người duyệt và lịch sử được mã hóa qua `IDbEncryptionService` của DbEncryption. Chỉ ID kỹ thuật, requestId và revision là cột rõ; không đặt thông tin định danh trong requestId. Data Protection keys ở `.keys` cần được giữ lại cùng database, bảo vệ bằng quyền hệ điều hành. Các file này đã được gitignore.

Mỗi lần ghi dùng optimistic concurrency; unique requestId bảo vệ cả các yêu cầu đồng thời. Endpoint chạy workflow trực tiếp, chưa cần Wolverine background hoặc Kafka. Một process bị dừng đột ngột có thể để lại `Generating`: đọc lịch sử để điều tra và tạo lần mới với requestId mới; chưa có worker tự phục hồi sau crash. `NeedsManualReview` là trạng thái chuyển cho con người, chưa có màn hình giải quyết riêng.

Mọi bác sĩ có secret được cấu hình đều truy cập các bệnh nhân giả lập. Đây là quyền truy cập demo, chưa phải phân quyền bệnh nhân hoặc đăng nhập production. Bản approved là kết quả lưu nội bộ, chưa tích hợp công bố vào EHR.

```powershell
dotnet test tests/VisitSummaries.Tests
```
