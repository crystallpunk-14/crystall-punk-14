from fluent import ftl_reader
import os


def ftl_parser(path: str) -> dict:
    """
        The function gets the path, then with the help of the os library
        goes through each file,checks that the file extension is "ftl",
        then reads it through the "ftl_reader" module of the "fluent" package.
    """
    prototypes = {}

    for dirpath, _, filenames in os.walk(path):
        for filename in filenames:
            path = f"{dirpath}/{filename}"

            if not filename.endswith(".ftl"):
                continue

            file = ftl_reader.read_ftl(path)
            prototypes.update(file)

    return prototypes
