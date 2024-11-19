from LocalizationHelper import get_logger, LocalizationHelper

YAML_FILES_PATH = "../../../Resources/Prototypes/_CP14/Entities"
FTL_FILES_PATH = "../../../Resources/Locale/ru-RU/_CP14/_PROTO/entities"

if __name__ == '__main__':
    logger = get_logger(__name__)

    logger.info("Starting...")
    helper = LocalizationHelper()
    helper.main(YAML_FILES_PATH, FTL_FILES_PATH)
