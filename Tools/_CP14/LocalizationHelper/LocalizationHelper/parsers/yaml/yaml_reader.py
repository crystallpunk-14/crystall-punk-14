import yaml

from LocalizationHelper import get_logger, LogText

logger = get_logger(__name__)

# taken from - https://github.com/poeMota/map-converter-ss14/blob/master/src/Yaml/yaml.py
def unknown_tag_constructor(loader, tag_suffix, node):
    if isinstance(node, yaml.ScalarNode):
        value = loader.construct_scalar(node)  # Node
    elif isinstance(node, yaml.SequenceNode):
        value = loader.construct_sequence(node)  # List
    elif isinstance(node, yaml.MappingNode):
        value = loader.construct_mapping(node)  # Dict
    else:
        raise TypeError(f"Unknown node type: {type(node)}")

    return {tag_suffix: value}


yaml.add_multi_constructor('!', unknown_tag_constructor)


def read_yaml(path: str) -> list[dict]:
    try:
        logger.debug("%s: %s", LogText.READING_DATA_FROM_FILE, path)
        with open(path, encoding="utf-8") as file:
            data = yaml.full_load(file)
        return data
    except Exception as e:
        logger.error("%s: %s - %s", LogText.ERROR_WHILE_READING_DATA_FROM_FILE, path, e, exc_info=True)
