from color_replacer import ColorReplacer
from pallets import *


# Del .rsi from dir name!
REPLACE_FROM = ""
REPLACE_TO = ""
REPLACE_FROM_PALLET = None # Chanhe None to your pallet!
REPLACE_TO_PALLET = None # Change None to your pallet!


replacer = ColorReplacer(REPLACE_FROM, REPLACE_TO, REPLACE_FROM_PALLET, REPLACE_TO_PALLET)
replacer.replace_colors()
