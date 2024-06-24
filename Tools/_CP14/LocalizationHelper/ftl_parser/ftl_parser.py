from fluent import ftl_reader
import os


def ftl_parser(path: str) -> dict:
    prototypes = {}

    for dirpath, _, filenames in os.walk(path):
        for filename in filenames:
            path = f"{dirpath}/{filename}"

            if not filename.endswith(".ftl"):
                continue

            file = ftl_reader.read_ftl(path)
            prototypes.update(file)

    return prototypes
