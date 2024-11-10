from unittest import TestCase, main
import os

from LocalizationHelper import YamlParser

yaml_files_path = os.path.join(os.path.dirname(__file__), 'data/yaml')
yaml_parser_result = YamlParser().get_prototypes(yaml_files_path)

VALID_PROTOTYPES_COUNT = 4
INVALID_PROTOTYPE_ID = "ForTest2"
EXPECTED_PROTOTYPES = {
    'CP14Fire': {
        'id': 'CP14Fire',
        'name': 'fire',
        'description': 'its fireee!!!',
        'parent': None,
        'suffix': 'cp14'
    },
    'CP14KeyTavern': {
        'id': 'CP14KeyTavern',
        'name': 'ключ от таверны',
        'description': None,
        'parent': 'CP14BaseKey',
        'suffix': None
    },
    'CP14Wallet': {
        'id': 'CP14Wallet',
        'name': 'wallet',
        'description': 'A small wallet, handy for storing coins.',
        'parent': 'BaseStorageItem',
        'suffix': None
    },
    'ForTest': {
        'id': 'ForTest',
        'name': None,
        'description': None,
        'parent': 'BaseItem',
        'suffix': None
    }
}


class YamlParserTests(TestCase):
    def test_result_len(self):
        self.assertEqual(len(yaml_parser_result), VALID_PROTOTYPES_COUNT,
                         f"This should result in only {VALID_PROTOTYPES_COUNT} prototypes.")

    def test_invalid_prototype_not_in_result(self):
        self.assertNotIn(INVALID_PROTOTYPE_ID, yaml_parser_result,
                         f"{INVALID_PROTOTYPE_ID} should not be present in the result.")

    def test_result_equals(self):
        prototypes_dict = {}
        for prototype_id, prototype_obj in yaml_parser_result.items():
            prototypes_dict[prototype_id] = prototype_obj.attrs_dict

        self.assertEqual(prototypes_dict, EXPECTED_PROTOTYPES)


if __name__ == '__main__':
    main()
