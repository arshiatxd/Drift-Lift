import cv2
import numpy as np
from PIL import Image
import os

xbox_dir = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\Xbox"
base_orig = Image.open(os.path.join(xbox_dir, "XB_Base.png")).convert("RGBA")

# Target canvas size: 420 x 260
canvas_w, canvas_h = 420, 260

# Uniform scaling
scale = 260.0 / 954.0 # 0.2725366876
scaled_w = int(round(1534.0 * scale)) # 418
scaled_h = 260
dx = (canvas_w - scaled_w) / 2.0 # 1.0
dy = 0.0

base_scaled = base_orig.resize((scaled_w, scaled_h), Image.Resampling.LANCZOS)
canvas = Image.new("RGBA", (canvas_w, canvas_h), (0, 0, 0, 255))
canvas.paste(base_scaled, (int(round(dx)), int(round(dy))), base_scaled)

# Exact 1534x954 coordinates for each element
# Let's define the exact (x, y, w, h) in 1534x954 space
elements = {
    # Face Buttons
    "XBSeries_A_Button.png": (1100, 439, 162, 157),
    "XBSeries_B_Button.png": (1208, 328, 166, 159),
    "XBSeries_X_Button.png": (987, 340, 169, 160),
    "XBSeries_Y_Button.png": (1094, 228, 175, 163),
    
    # D-Pad Arms
    "XBSeries_D-PAD_Up.png": (523, 565, 110, 100),
    "XBSeries_D-PAD_Down.png": (525, 665, 105, 105),
    "XBSeries_D-PAD_Left.png": (478, 605, 126, 100),
    "XBSeries_D-PAD_Right.png": (578, 606, 123, 98),
    
    # Bumpers
    "XBSeries_LeftBumper_Active.png": (140, 155, 498, 172),
    "XBSeries_RightBumper_Active.png": (896, 155, 501, 171),
    
    # Triggers
    "XBSeries_LeftTrigger_Active.png": (240, 0, 245, 190),
    "XBSeries_RightTrigger_Active.png": (1050, 0, 241, 168),
    
    # System buttons
    "XBSeries_ViewButton.png": (594, 372, 115, 106),
    "XBSeries_MenuButton.png": (825, 372, 115, 106),
    "XBSeries_ShareButton.png": (697, 471, 136, 77),
    "XBSeries_HomeButton.png": (673, 219, 187, 177),
    
    # Sticks
    "XBSeries_LeftStick.png": (278, 330, 177, 176),
    "XBSeries_RightStick.png": (884, 573, 180, 170),
}

xaml_lines = []
for fname, (bx, by, bw, bh) in elements.items():
    cx = dx + bx * scale
    cy = dy + by * scale
    cw = bw * scale
    ch = bh * scale
    
    # Overlay on test canvas
    p = os.path.join(xbox_dir, fname)
    if os.path.exists(p):
        btn = Image.open(p).convert("RGBA")
        btn_resized = btn.resize((int(round(cw)), int(round(ch))), Image.Resampling.LANCZOS)
        canvas.alpha_composite(btn_resized, (int(round(cx)), int(round(cy))))
    
    xaml_lines.append(f'<!-- {fname} --> Left="{cx:.1f}" Top="{cy:.1f}" Width="{cw:.1f}" Height="{ch:.1f}"')

out_canvas_path = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\scripts\test_canvas_420x260.png"
canvas.save(out_canvas_path)
print(f"Saved {out_canvas_path}")

for l in xaml_lines:
    print(l)
