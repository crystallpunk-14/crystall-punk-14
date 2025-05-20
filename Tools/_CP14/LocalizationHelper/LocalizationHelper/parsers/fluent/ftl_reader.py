from LocalizationHelper import get_logger, LogText

logger = get_logger(__name__)


def read_ftl(path: str) -> dict:

    prototypes = {}

    last_prototype = ""
    try:
        logger.debug("%s: %s", LogText.READING_DATA_FROM_FILE, path)
        with open(path, encoding="utf-8") as file:
            for line in file.readlines():
                if line.strip().startswith("#") or line.strip() == '':
                    continue

                if not line.startswith(" "): 
                    proto_id, proto_name = line.split(" = ")
                    proto_id = proto_id.replace("ent-", "")
                    last_prototype = proto_id
                    prototypes[proto_id] = {
                        "id": proto_id,
                        "name": proto_name.strip(),
                        "description": None,
                        "suffix": None
                    }
                else:
                    if "desc" in line:
                        attr = "description"
                    elif "suffix" in line:
                        attr = "suffix"

                    prototypes[last_prototype][attr] = line.split(" = ", 1)[1].strip()
    except Exception as e:
        logger.error("%s: %s - %s", LogText.ERROR_WHILE_READING_DATA_FROM_FILE, path, e, exc_info=True)
    else:
        return prototypes
