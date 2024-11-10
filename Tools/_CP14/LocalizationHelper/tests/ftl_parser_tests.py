from unittest import TestCase, main
import os

from LocalizationHelper import FtlParser

ftl_files_path = os.path.join(os.path.dirname(__file__), 'data/ftl')
ftl_parser_result = FtlParser().get_prototypes(ftl_files_path)

EXCEPTED_RESULT = {
    'CP14Fire': {
        'id': 'CP14Fire',
        'name': 'fire',
        'description': 'its fireee!!!',
        'parent': None,
        'suffix': 'cp14'
    },
    'CP14BaseKeyRing': {
        'id': 'CP14BaseKeyRing',
        'name': 'кольцо для ключей',
        'description': 'Позволяет комфортно хранить большое количество ключей в одном месте.',
        'parent': None,
        'suffix': 'Пустое'
    },
    'CP14KeyRingInnkeeper': {
        'id': 'CP14KeyRingInnkeeper',
        'name': '{ ent-CP14BaseKeyRing }',
        'description': '{ ent-CP14BaseKeyRing.desc }',
        'parent': None,
        'suffix': 'Трактирщик'
    },
    'CP14BaseKey': {
        'id': 'CP14BaseKey',
        'name': 'ключик',
        'description': 'Небольшая заковыристая железяка, открывающая определенные замки. Не отдавайте кому попало!',
        'parent': None,
        'suffix': None
    },
    'CP14BaseLockpick': {
        'id': 'CP14BaseLockpick',
        'name': 'отмычка',
        'description': 'Воровской инструмент, при должном умении и сноровке позволяющий взламывать любые замки.',
        'parent': None,
        'suffix': None
    },
    'CP14KeyTavern': {
        'id': 'CP14KeyTavern',
        'name': 'ключ от таверны',
        'description': '{ ent-CP14BaseKey.desc }',
        'parent': None,
        'suffix': None
    },
    'CP14BaseLock': {
        'id': 'CP14BaseLock',
        'name': 'стальной замок',
        'description': 'Он запирает вещи. И вам потребуется ключ, чтобы открыть их обратно.',
        'parent': None,
        'suffix': None
    },
    'CP14LockTavern': {
        'id': 'CP14LockTavern',
        'name': 'замок от таверны',
        'description': '{ ent-CP14BaseLock.desc }',
        'parent': None,
        'suffix': None
    },
    'CP14DirtBlock1': {
        'id': 'CP14DirtBlock1',
        'name': 'блок земли',
        'description': None,
        'parent': None,
        'suffix': None
    },
    'CP14DirtBlock10': {
        'id': 'CP14DirtBlock10',
        'name': '{ ent-CP14DirtBlock1 }',
        'description': None,
        'parent': None,
        'suffix': None
    },
    'CP14StoneBlock1': {
        'id': 'CP14StoneBlock1',
        'name': 'stone block',
        'description': 'A block of cold stone.',
        'parent': None,
        'suffix': None
    },
    'CP14StoneBlock10': {
        'id': 'CP14StoneBlock10',
        'name': '{ ent-CP14StoneBlock1 }',
        'description': '{ ent-CP14StoneBlock1.desc }',
        'parent': None,
        'suffix': '10'
    }
}


class FtlParserTests(TestCase):

    def test_result_equals(self):
        prototypes_dict = {}
        for prototype_id, prototype_obj in ftl_parser_result.items():
            prototypes_dict[prototype_id] = prototype_obj.attrs_dict
        self.assertEqual(prototypes_dict, EXCEPTED_RESULT)


if __name__ == '__main__':
    main()
