import unittest

from mod_builder import modify, find_parameter_object


class ModBuilderTests(unittest.TestCase):
    def setUp(self):
        self.data = {"Clothes": {"SortID": 1, "ModelName": "original.fmdb",
                                 "VariationColor": {"R": 1.0, "G": 1.0, "B": 1.0, "A": 1.0}}}

    def test_edit_does_not_mutate_input(self):
        result = modify(self.data, sort_id=10, color=(0.1, 0.2, 0.3, 1.0))
        self.assertEqual(find_parameter_object(result)["SortID"], 10)
        self.assertEqual(find_parameter_object(result)["VariationColor"]["G"], 0.2)
        self.assertEqual(find_parameter_object(self.data)["SortID"], 1)

    def test_reject_invalid_color(self):
        with self.assertRaises(ValueError):
            modify(self.data, color=(2.0, 0, 0, 1))

    def test_reject_ambiguous_targets(self):
        with self.assertRaises(ValueError):
            find_parameter_object([self.data, self.data])

    def test_edit_model(self):
        result = modify(self.data, model_name="alternative.fmdb")
        self.assertEqual(find_parameter_object(result)["ModelName"], "alternative.fmdb")


if __name__ == "__main__":
    unittest.main()
