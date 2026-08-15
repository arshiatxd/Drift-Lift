import cv2
import numpy as np
from PIL import Image
import os

xbox_dir = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\Xbox"
base = Image.open(os.path.join(xbox_dir, "XB_Base.png")).convert("RGBA")

# Let's test placing each active press button on base (at 1534x954 space):
# Let's verify each button's exact matching position on 1534x954

# For LB, RB, LT, RT:
# Let's find the exact pixel match by doing alpha overlay test!
test_img = base.copy()

buttons = [
    ("XBSeries_LeftTrigger_Active.png", 240, 0),
    ("XBSeries_RightTrigger_Active.png", 1050, 0),
    ("XBSeries_LeftBumper_Active.png", 140, 155),
    ("XBSeries_RightBumper_Active.png", 896, 155),
    ("XBSeries_A_Button.png", 1100, 439),
    ("XBSeries_B_Button.png", 1208, 328),
    ("XBSeries_X_Button.png", 987, 340),
    ("XBSeries_Y_Button.png", 1094, 228),
    ("XBSeries_D-PAD_Up.png", 523, 565),
    ("XBSeries_D-PAD_Down.png", 525, 665),
    ("XBSeries_D-PAD_Left.png", 478, 605),
    ("XBSeries_D-PAD_Right.png", 578, 606),
    ("XBSeries_ViewButton.png", 594, 372),
    ("XBSeries_MenuButton.png", 825, 372),
    ("XBSeries_ShareButton.png", 697, 471),
    ("XBSeries_HomeButton.png", 673, 219),
]

for fname, x, y in buttons:
    p = os.path.join(xbox_dir, fname)
    if os.path.exists(p):
        btn = Image.open(p).convert("RGBA")
        test_img.alpha_composite(btn, (int(x), int(y)))

out_path = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\scripts\test_composite.png"
test_img.save(out_path)
print(f"Saved {out_path} ({test_img.size})")
