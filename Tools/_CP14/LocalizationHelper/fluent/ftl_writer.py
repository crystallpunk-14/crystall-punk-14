import json


def create_ftl(key: str, prototype: dict) -> str:
    ftl = ""

    ftl += f"ent-{key} = {prototype["name"]}\n"

    if prototype["desc"] is not None:
        ftl += f"    .desc = {prototype["desc"]}\n"

    if prototype["suffix"] is not None:
        ftl += f"    .suffix = {prototype["suffix"]}\n"

    ftl += "\n"

    return ftl
