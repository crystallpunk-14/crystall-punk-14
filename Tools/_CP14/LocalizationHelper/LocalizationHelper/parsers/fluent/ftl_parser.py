from . import read_ftl
from LocalizationHelper import get_logger, LogText
from LocalizationHelper.entity import Entity
from LocalizationHelper.parsers import BaseParser

logger = get_logger(__name__)


class FtlParser(BaseParser):
    def __init__(self):
        logger.debug("%s FtlParser", LogText.CLASS_INITIALIZATION)

    def get_prototypes(self, ftl_prototypes_path: str) -> dict[str, Entity]:
        prototypes = {}
        ftl_prototypes_files_path = self._get_files_paths_in_dir(ftl_prototypes_path)

        for prototype_file_path in ftl_prototypes_files_path:
            if not self._check_file_extension(prototype_file_path, "ftl"):
                continue

            file_prototypes_dict = read_ftl(prototype_file_path)

            for prototype_dict in file_prototypes_dict.values():
                prototype_obj = Entity(prototype_dict)
                logger.debug("%s: %s", LogText.HAS_BEEN_PROCESSED, prototype_obj)
                prototypes[prototype_obj.id] = prototype_obj

        return prototypes
