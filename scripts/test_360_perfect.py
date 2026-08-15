import cv2
import numpy as np
from PIL import Image
import os

base_path = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\Xbox360\XB360_Base.png"
press_dir = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\Xbox360"

base = Image.open(base_path).convert("RGBA")
W, H = base.size # 1545, 955

# Left stick well center:
# In the base image, left stick outer ring is centered at:
# Let's inspect the exact center of the Left Stick Well (x in 150..450, y in 350..650):
base_cv = cv2.imread(base_path, cv2.IMREAD_UNCHANGED)
gray = cv2.cvtColor(base_cv[:,:,:3], cv2.COLOR_BGR2GRAY)

# Stick well centers:
# Let's find circle centers:
# Left stick well outer ring: x ~ 303, y ~ 463
# Right stick well outer ring: x ~ 945, y ~ 692
ls_center_x, ls_center_y = 303.0, 463.0
rs_center_x, rs_center_y = 945.0, 692.0

# Guide button center:
guide_x, guide_y = 774.0, 474.0

# Back / Start button centers:
back_x, back_y = 595.0, 474.0
start_x, start_y = 953.0, 474.0

# ABXY centers:
y_x, y_y = 1254.0, 373.0
b_x, b_y = 1378.0, 474.0
x_x, x_y = 1130.0, 474.0
a_x, a_y = 1254.0, 575.0

# Scale to Canvas 420x260:
scale = 260.0 / 955.0 # 0.2722513
dx = (420.0 - 1545.0 * scale) / 2.0 # -0.31
dy = 0.0

items = [
    # BUMPERS & TRIGGERS (Exact edge match coordinates):
    ("XB360_LeftBumper_Active.png", 135, 128, 312, 141),
    ("XB360_RightBumper_Active.png", 1129, 125, 285, 141),
    ("XB360_LeftTrigger_Active.png", 277, 8, 143, 152),
    ("XB360_RightTrigger_Active.png", 1152, 6, 143, 152),

    # D-PAD (Exact edge match coordinates):
    ("XB360_D-PAD_Up.png", 487, 608, 108, 114),
    ("XB360_D-PAD_Down.png", 487, 721, 108, 114),
    ("XB360_D-PAD_Left.png", 407, 674, 134, 108),
    ("XB360_D-PAD_Right.png", 538, 668, 134, 107),

    # ABXY Buttons:
    ("XB360_Y_Button.png", y_x - 129/2, y_y - 118/2, 129, 118),
    ("XB360_B_Button.png", b_x - 122/2, b_y - 115/2, 122, 115),
    ("XB360_X_Button.png", x_x - 126/2, x_y - 113/2, 126, 113),
    ("XB360_A_Button.png", a_x - 127/2, a_y - 106/2, 127, 106),

    # Guide, Back, Start:
    ("XB360_GuideButton.png", guide_x - 171/2, guide_y - 139/2, 171, 139),
    ("XB360_BackButton.png", back_x - 92/2, back_y - 65/2, 92, 65),
    ("XB360_StartButton.png", start_x - 92/2, start_y - 65/2, 92, 65),

    # Analog Stick caps (stick diameter ~ 180px in 1545x955 space):
    ("XB360_LeftStick_Black.png", ls_center_x - 180/2, ls_center_y - 180/2, 180, 180),
    ("XB360_RightStick_Black.png", rs_center_x - 180/2, rs_center_y - 180/2, 180, 180),
]

scaled_w = int(round(1545.0 * scale))
base_scaled = base.resize((scaled_w, 260), Image.Resampling.LANCZOS)
canvas = Image.new("RGBA", (420, 260), (0, 0, 0, 255))
canvas.paste(base_scaled, (int(round(dx)), int(round(dy))), base_scaled)

for fname, bx, by, bw, bh in items:
    cx = dx + bx * scale
    cy = dy + by * scale
    cw = bw * scale
    ch = bh * scale
    p = os.path.join(press_dir, fname)
    if os.path.exists(p):
        btn = Image.open(p).convert("RGBA")
        btn_r = btn.resize((int(round(cw)), int(round(ch))), Image.Resampling.LANCZOS)
        canvas.alpha_composite(btn_r, (int(round(cx)), int(round(cy))))
    print(f"{fname:30s} -> Canvas.Left=\"{cx:5.1f}\" Canvas.Top=\"{cy:5.1f}\" Width=\"{cw:4.1f}\" Height=\"{ch:4.1f}\"")

canvas.save(r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\scripts\test_360_perfect.png")
