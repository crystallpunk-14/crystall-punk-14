import logging
import os


LOG_FILE_PATH = os.path.join("logs", "helper.log")
os.makedirs(os.path.dirname(LOG_FILE_PATH), exist_ok=True)

file_handler = logging.FileHandler(LOG_FILE_PATH,  mode='w', encoding="utf-8")
file_handler.setLevel(logging.DEBUG)

console_handler = logging.StreamHandler()
console_handler.setLevel(logging.INFO)

logging.basicConfig(
    level=logging.DEBUG,
    format="%(asctime)s - %(name)s - %(levelname)s - %(message)s",
    handlers=[
        file_handler,
        console_handler
    ]
)


def get_logger(name: str) -> logging.Logger:
    return logging.getLogger(name)


class LogText:
    CLASS_INITIALIZATION = "initializing"
    GETTING_PROTOTYPES_FROM_YAML = "Getting prototypes from YAML files"
    GETTING_PROTOTYPES_FROM_FTL = "Getting prototypes from FTL files"
    FORMING_FINAL_DICTIONARY = "Forming the final prototypes dictionary"
    SAVING_LAST_LAUNCH_RESULT = "Saving yaml_parser result to file"
    READING_LAST_LAUNCH_RESULT = "Reading data from last launch"
    SAVING_FINAL_RESULT = "Saving the final prototypes dictionary to FTL file"
    SAVING_DATA_TO_FILE = "Saving data to file"
    READING_DATA_FROM_FILE = "Reading data from file"
    ERROR_WHILE_WRITING_DATA_TO_FILE = "Error while writing data to a file"
    ERROR_WHILE_READING_DATA_FROM_FILE = "Error while writing data to a file"
    UNKNOWN_ERROR = "An error occurred during execution"
    HAS_BEEN_DELETED = "Has been deleted due to lack of relevant data"
    HAS_BEEN_PROCESSED = "Has been processed"
    FORMING_FTL_FOR_PROTOTYPE = "Forming FTL for Entity"
