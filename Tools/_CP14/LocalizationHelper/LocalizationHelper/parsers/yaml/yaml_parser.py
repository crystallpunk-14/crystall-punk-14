from . import read_yaml
from LocalizationHelper import get_logger, LogText
from LocalizationHelper.entity import Entity, check_prototype_attrs
from LocalizationHelper.parsers import BaseParser

logger = get_logger(__name__)


class YamlParser(BaseParser):
    def __init__(self):
        logger.debug("%s YamlParser", LogText.CLASS_INITIALIZATION)

    @staticmethod
    def _check_prototypes_with_multiple_parents(prototypes: dict[str, Entity],
                                                prototypes_with_multiple_parents: dict[str, Entity]) -> dict[str, Entity]:

        for prototype_obj in prototypes_with_multiple_parents.values():
            available_parents = []
            for parent in prototype_obj.parent:
                if parent in prototypes:
                    available_parents.append(parent)

            if len(available_parents) >= 2:
                prototype_obj.parent = available_parents
            elif len(available_parents) == 0:
                prototype_obj.parent = None
            else:
                prototype_obj.parent = available_parents[0]

            logger.debug("%s: %s", LogText.HAS_BEEN_PROCESSED, prototype_obj)
            prototypes[prototype_obj.id] = prototype_obj

        return prototypes

    def get_prototypes(self, yaml_prototypes_path: str) -> dict[str, Entity]:
        prototypes: dict[str, Entity] = {}
        prototypes_with_multiple_parents: dict[str, Entity] = {}

        yaml_prototypes_files_path = self._get_files_paths_in_dir(yaml_prototypes_path)

        for prototype_file_path in yaml_prototypes_files_path:
            if not self._check_file_extension(prototype_file_path, "yml"):
                continue

            file_prototypes_list: list[dict] = read_yaml(prototype_file_path)
            if file_prototypes_list:
                for prototype_dict in file_prototypes_list:
                    prototype_type = prototype_dict.get("type")
                    if prototype_type == "entity":
                        prototype_obj = Entity(prototype_dict)
                        if check_prototype_attrs(prototype_obj):
                            if isinstance(prototype_obj.parent, list):
                                prototypes_with_multiple_parents[prototype_obj.id] = prototype_obj
                                continue

                            logger.debug("%s: %s", LogText.HAS_BEEN_PROCESSED, prototype_obj)
                            prototypes[prototype_obj.id] = prototype_obj

        prototypes = self._check_prototypes_with_multiple_parents(prototypes, prototypes_with_multiple_parents)

        return prototypes
