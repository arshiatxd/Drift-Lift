import cv2
import numpy as np
from PIL import Image
import os

xbox_dir = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\Xbox"
base = Image.open(os.path.join(xbox_dir, "XB_Base.png")).convert("RGBA")

# Let's inspect Left Bumper
lb_img = Image.open(os.path.join(xbox_dir, "XBSeries_LeftBumper_Active.png")).convert("RGBA")
rb_img = Image.open(os.path.join(xbox_dir, "XBSeries_RightBumper_Active.png")).convert("RGBA")
lt_img = Image.open(os.path.join(xbox_dir, "XBSeries_LeftTrigger_Active.png")).convert("RGBA")
rt_img = Image.open(os.path.join(xbox_dir, "XBSeries_RightTrigger_Active.png")).convert("RGBA")

# Let's find bumper contour on base
# On base, the left bumper runs along x: 140..630, y: 155..315 (size ~ 498x172)
# On base, the right bumper runs along x: 900..1395, y: 155..315 (size ~ 501x171)
# On base, left trigger is at x: 235..480, y: 0..190 (size ~ 245x190)
# On base, right trigger is at x: 1055..1295, y: 0..168 (size ~ 241x168)

print("LB size in 1534x954:", lb_img.size)
print("RB size in 1534x954:", rb_img.size)
print("LT size in 1534x954:", lt_img.size)
print("RT size in 1534x954:", rt_img.size)

# Let's test overlaying at these positions and check edge alignment!
scale = 260.0 / 954.0
dx = (420.0 - 1534.0 * scale) / 2.0
dy = 0.0

def to_canvas(bx, by, bw, bh):
    cx = dx + bx * scale
    cy = dy + by * scale
    cw = bw * scale
    ch = bh * scale
    return cx, cy, cw, ch

# Let's calculate the positions
print("\n--- Canvas Coordinates (420x260) ---")
# ABXY (using actual button centers and properly scaled sizes):
# In 1534x954, button size is 162x157 -> scaled: 162*scale = 44.1, 157*scale = 42.8.
# BUT on the controller drawing, the button face is only diameter ~88px (which is 88*scale = 24.0px),
# and the active press image XBSeries_A_Button.png (162x157) includes the outer cyan glow halo!
# When XBSeries_A_Button.png (162x157) is centered at A center (1181, 518):
# Base TopLeft = (1181 - 162/2, 518 - 157/2) = (1100.0, 439.5)
# Canvas:
for name, bx, by, bw, bh in [
    ("A Button", 1181 - 81, 518 - 78.5, 162, 157),
    ("B Button", 1291 - 83, 408 - 79.5, 166, 159),
    ("X Button", 1072 - 84.5, 420 - 80, 169, 160),
    ("Y Button", 1182 - 87.5, 310 - 81.5, 175, 163),
    
    # D-Pad: D-pad center is (578, 655).
    # Up:
    ("D-Pad Up", 578 - 55, 655 - 90, 110, 100),
    ("D-Pad Down", 578 - 52.5, 655 + 10, 105, 105),
    ("D-Pad Left", 578 - 100, 655 - 50, 126, 100),
    ("D-Pad Right", 578 + 0, 655 - 49, 123, 98),
    
    # Bumpers:
    ("Left Bumper", 140, 155, 498, 172),
    ("Right Bumper", 896, 155, 501, 171),
    
    # Triggers:
    ("Left Trigger", 240, 0, 245, 190),
    ("Right Trigger", 1050, 0, 241, 168),
    
    # Center buttons:
    ("View", 652 - 57.5, 425 - 53, 115, 106),
    ("Menu", 883 - 57.5, 425 - 53, 115, 106),
    ("Share", 765 - 68, 510 - 38.5, 136, 77),
    ("Home/Guide", 767 - 93.5, 308 - 88.5, 187, 177),
]:
    cx, cy, cw, ch = to_canvas(bx, by, bw, bh)
    print(f"{name:15s}: Canvas.Left=\"{cx:5.1f}\" Canvas.Top=\"{cy:5.1f}\" Width=\"{cw:4.1f}\" Height=\"{ch:4.1f}\"")
