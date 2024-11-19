from . import read_yaml
from LocalizationHelper import get_logger, LogText
from LocalizationHelper.prototype import Prototype, check_prototype_attrs
from LocalizationHelper.parsers import BaseParser

logger = get_logger(__name__)


class YamlParser(BaseParser):
    def __init__(self):
        logger.debug("%s YamlParser", LogText.CLASS_INITIALIZATION)

    def get_prototypes(self, yaml_prototypes_path: str) -> dict[str, Prototype]:
        prototypes = {}
        yaml_prototypes_files_path = self._get_files_paths_in_dir(yaml_prototypes_path)

        for prototype_file_path in yaml_prototypes_files_path:
            if not self._check_file_extension(prototype_file_path, "yml"):
                continue

            file_prototypes_list: list[dict] = read_yaml(prototype_file_path)
            if file_prototypes_list:
                for prototype_dict in file_prototypes_list:
                    prototype_obj = Prototype(prototype_dict)
                    if check_prototype_attrs(prototype_obj):
                        logger.debug("%s: %s", LogText.HAS_BEEN_PROCESSED, prototype_obj)
                        prototypes[prototype_obj.id] = prototype_obj

        return prototypes
