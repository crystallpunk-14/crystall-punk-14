from fluent import ftl_reader
from base_parser import BaseParser


class FTLParser(BaseParser):
    """
    The class inherits from the "BaseParser" class, parses ftl files of localization.
    """

    def ftl_parser(self) -> dict:
        """
            The function gets the path, then with the help of the os library
            goes through each file,checks that the file extension is "ftl",
            then reads it through the "ftl_reader" module of the "fluent" package.
        """
        prototypes = {}

        for path in self.get_files_paths():

            if not self.check_file_extension(path, ".ftl"):
                continue

            file = ftl_reader.read_ftl((path, self.errors_path))
            prototypes.update(file)

        return prototypes
