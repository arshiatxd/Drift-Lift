import cv2
import numpy as np
from PIL import Image
import os

xb360_dir = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\Xbox360"
base = Image.open(os.path.join(xb360_dir, "XB360_Base.png")).convert("RGBA")

scale = 260.0 / 955.0 # 0.2722513
dx = (420.0 - 1545.0 * scale) / 2.0 # -0.31
dy = 0.0

scaled_w = int(round(1545.0 * scale))
base_scaled = base.resize((scaled_w, 260), Image.Resampling.LANCZOS)
canvas = Image.new("RGBA", (420, 260), (0, 0, 0, 255))
canvas.paste(base_scaled, (int(round(dx)), int(round(dy))), base_scaled)

elements_360_fine = [
    # ABXY Buttons:
    ("XB360_Y_Button.png", 323.7, 85.5, 35.1, 32.1),
    ("XB360_B_Button.png", 356.6, 112.6, 33.2, 31.3),
    ("XB360_X_Button.png", 287.5, 114.8, 34.3, 30.8),
    ("XB360_A_Button.png", 321.9, 143.5, 34.6, 28.9),

    # D-Pad:
    ("XB360_D-PAD_Up.png", 137.0, 159.0, 24.0, 23.0),
    ("XB360_D-PAD_Down.png", 137.0, 180.0, 24.0, 23.0),
    ("XB360_D-PAD_Left.png", 125.0, 169.0, 26.0, 23.0),
    ("XB360_D-PAD_Right.png", 147.0, 169.0, 26.0, 23.0),

    # Bumpers:
    ("XB360_LeftBumper_Active.png", 70.5, 42.2, 84.9, 38.4),
    ("XB360_RightBumper_Active.png", 265.1, 42.2, 77.6, 38.4),

    # Triggers:
    ("XB360_LeftTrigger_Active.png", 75.9, 0.0, 38.9, 41.4),
    ("XB360_RightTrigger_Active.png", 304.6, 0.0, 38.9, 41.4),

    # System:
    ("XB360_BackButton.png", 149.2, 120.2, 25.0, 17.7),
    ("XB360_StartButton.png", 246.6, 120.2, 25.0, 17.7),
    ("XB360_GuideButton.png", 187.1, 110.1, 46.6, 37.8),

    # Sticks:
    ("XB360_LeftStick_Black.png", 71.0, 88.5, 49.0, 49.0),
    ("XB360_RightStick_Black.png", 237.9, 158.5, 49.0, 49.0),
]

for fname, cx, cy, cw, ch in elements_360_fine:
    p = os.path.join(xb360_dir, fname)
    if os.path.exists(p):
        btn = Image.open(p).convert("RGBA")
        btn_r = btn.resize((int(round(cw)), int(round(ch))), Image.Resampling.LANCZOS)
        canvas.alpha_composite(btn_r, (int(round(cx)), int(round(cy))))

canvas.save(r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\scripts\test_360_canvas_fine.png")
print("Saved test_360_canvas_fine.png")
