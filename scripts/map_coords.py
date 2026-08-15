"""
Controller button coordinate mapper.
Reads the controller images, displays them at 420x260 scale, 
and prints the WPF Canvas coordinates for each button.

Xbox image: 1386x924 -> display 420x260
PS4 image:  1386x924 -> display 420x260

Scale factors:
  scaleX = 420 / 1386 = 0.3030
  scaleY = 260 / 924  = 0.2814

Button positions measured from the actual images (pixel coordinates in original resolution):
"""

SX = 420 / 1386  # 0.3030
SY = 260 / 924   # 0.2814

BUTTON_SIZE = 28  # display px for round buttons
HALF = BUTTON_SIZE // 2

def coord(px, py, label):
    """Convert original image pixel to canvas Left,Top (top-left of button)"""
    cx = round(px * SX - HALF)
    cy = round(py * SY - HALF)
    print(f"  {label:20s} Canvas.Left={cx:4d}  Canvas.Top={cy:4d}  (center: {round(px*SX)},{round(py*SY)})")
    return cx, cy

print("=" * 60)
print("XBOX Controller buttons (420x260 canvas):")
print("=" * 60)
# Measured from xbox_controller_flat image (1386x924)
# Left stick center
coord(330, 290, "Left Stick")
# Right stick center  
coord(720, 420, "Right Stick")
# D-pad center
coord(390, 430, "D-pad center")
coord(390, 390, "  D-pad Up")
coord(390, 470, "  D-pad Down")
coord(350, 430, "  D-pad Left")
coord(430, 430, "  D-pad Right")
# Face buttons cluster: Y=top, A=bottom, X=left, B=right
# Center of cluster ~(940, 300), spacing ~55px
coord(940, 245, "Y (top)")
coord(940, 355, "A (bottom)")
coord(885, 300, "X (left)")
coord(995, 300, "B (right)")
# View/Menu
coord(580, 290, "View button")
coord(700, 290, "Menu button")
# Xbox guide
coord(640, 210, "Xbox guide")

print()
print("=" * 60)
print("PS4 Controller buttons (420x260 canvas):")
print("=" * 60)
# Measured from ps4_controller_flat image (1386x924)
# Left stick center (bottom-left symmetric area)
coord(420, 490, "Left Stick")
# Right stick center
coord(750, 490, "Right Stick")
# D-pad center (upper-left quadrant)
coord(230, 310, "D-pad center")
coord(230, 255, "  D-pad Up")
coord(230, 365, "  D-pad Down")
coord(175, 310, "  D-pad Left")
coord(285, 310, "  D-pad Right")
# Touchpad
print(f"  {'Touchpad':20s} Canvas.Left={round(470*SX):4d}  Canvas.Top={round(175*SY):4d}  (width={round(370*SX)} height={round(185*SY)})")
# Share button
coord(465, 175, "Share")
# Options button
coord(850, 175, "Options")
# Face buttons: Triangle=top, Cross=bottom, Square=left, Circle=right
# Center of cluster ~(1055, 310), spacing ~65px
coord(1055, 240, "Triangle (top)")
coord(1055, 375, "Cross (bottom)")
coord(990, 310, "Square (left)")
coord(1120, 310, "Circle (right)")
