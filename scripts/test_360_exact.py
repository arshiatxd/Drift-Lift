import cv2
import numpy as np
from PIL import Image
import os

xb360_dir = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\Xbox360"
base = Image.open(os.path.join(xb360_dir, "XB360_Base.png")).convert("RGBA")

# 1545x955 space exact centers:
# ABXY:
y_cx, y_cy = 1254.5, 373.0
b_cx, b_cy = 1372.0, 471.0
x_cx, x_cy = 1120.0, 478.0
a_cx, a_cy = 1247.0, 580.0

# Guide, Back, Start:
guide_cx, guide_cy = 774.0, 474.0
back_cx, back_cy = 595.0, 474.0
start_cx, start_cy = 953.0, 474.0

# Sticks & D-Pad:
ls_cx, ls_cy = 352.0, 415.0
rs_cx, rs_cy = 965.0, 672.0
dpad_cx, dpad_cy = 548.0, 672.0

scale = 260.0 / 955.0 # 0.2722513
dx = (420.0 - 1545.0 * scale) / 2.0 # -0.31
dy = 0.0

elements_360 = [
    # ABXY Buttons:
    ("XB360_Y_Button.png", y_cx - 129/2, y_cy - 118/2, 129, 118),
    ("XB360_B_Button.png", b_cx - 122/2, b_cy - 115/2, 122, 115),
    ("XB360_X_Button.png", x_cx - 126/2, x_cy - 113/2, 126, 113),
    ("XB360_A_Button.png", a_cx - 127/2, a_cy - 106/2, 127, 106),

    # D-Pad:
    ("XB360_D-PAD_Up.png", dpad_cx - 108/2, dpad_cy - 85, 108, 114),
    ("XB360_D-PAD_Down.png", dpad_cx - 108/2, dpad_cy - 20, 108, 114),
    ("XB360_D-PAD_Left.png", dpad_cx - 95, dpad_cy - 108/2, 134, 108),
    ("XB360_D-PAD_Right.png", dpad_cx - 35, dpad_cy - 107/2, 134, 107),

    # Bumpers:
    ("XB360_LeftBumper_Active.png", 260, 155, 312, 141),
    ("XB360_RightBumper_Active.png", 975, 155, 285, 141),

    # Triggers:
    ("XB360_LeftTrigger_Active.png", 280, 0, 143, 152),
    ("XB360_RightTrigger_Active.png", 1120, 0, 143, 152),

    # System:
    ("XB360_BackButton.png", back_cx - 92/2, back_cy - 65/2, 92, 65),
    ("XB360_StartButton.png", start_cx - 92/2, start_cy - 65/2, 92, 65),
    ("XB360_GuideButton.png", guide_cx - 171/2, guide_cy - 139/2, 171, 139),

    # Sticks:
    ("XB360_LeftStick_Black.png", ls_cx - 180/2, ls_cy - 180/2, 180, 180),
    ("XB360_RightStick_Black.png", rs_cx - 180/2, rs_cy - 180/2, 180, 180),
]

scaled_w = int(round(1545.0 * scale))
base_scaled = base.resize((scaled_w, 260), Image.Resampling.LANCZOS)
canvas = Image.new("RGBA", (420, 260), (0, 0, 0, 255))
canvas.paste(base_scaled, (int(round(dx)), int(round(dy))), base_scaled)

for fname, bx, by, bw, bh in elements_360:
    cx = dx + bx * scale
    cy = dy + by * scale
    cw = bw * scale
    ch = bh * scale
    p = os.path.join(xb360_dir, fname)
    if os.path.exists(p):
        btn = Image.open(p).convert("RGBA")
        btn_r = btn.resize((int(round(cw)), int(round(ch))), Image.Resampling.LANCZOS)
        canvas.alpha_composite(btn_r, (int(round(cx)), int(round(cy))))
    print(f"{fname:30s} -> Left=\"{cx:5.1f}\" Top=\"{cy:5.1f}\" Width=\"{cw:4.1f}\" Height=\"{ch:4.1f}\"")

canvas.save(r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\scripts\test_360_canvas.png")
