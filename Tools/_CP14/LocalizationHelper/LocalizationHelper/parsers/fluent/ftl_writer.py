from LocalizationHelper import get_logger, LogText
from LocalizationHelper.prototype import Prototype

logger = get_logger(__name__)

INDENT = "    "


def create_ftl(prototype: Prototype) -> str:
    logger.debug("%s: %s", LogText.FORMING_FTL_FOR_PROTOTYPE, prototype.attrs_dict)
    ftl = ""

    ftl += f"ent-{prototype.id} = {prototype.name}\n"

    if prototype.description:
        ftl += f"{INDENT}.desc = {prototype.description}\n"

    if prototype.suffix:
        ftl += f"{INDENT}.suffix = {prototype.suffix}\n"

    ftl += "\n"

    return ftl
