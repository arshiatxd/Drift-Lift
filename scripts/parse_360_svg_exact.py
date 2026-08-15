import xml.etree.ElementTree as ET
import os

svg_path = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox 360 Controller Images\Default Theme\Theme SVG\Xbox 360 VSCView - Black.svg"
tree = ET.parse(svg_path)
root = tree.getroot()

# The SVG has viewBox "0 0 408.81619 252.84766"
# Let's map elements from mm/viewbox to 1545x955!
# 1545 / 408.81619 = 3.779155
# 955 / 252.84766 = 3.7770
# 3.779527559 pixels per mm (96 DPI standard in SVG!)

scale_x = 1545.0 / 408.81619
scale_y = 955.0 / 252.84766

print(f"SVG to Base scale: x={scale_x}, y={scale_y}")

for elem in root.iter():
    lbl = elem.attrib.get('{http://www.inkscape.org/namespaces/inkscape}label', '')
    eid = elem.attrib.get('id', '')
    tag = elem.tag.split('}')[-1]
    
    # Check if this element has bounding box or path data
    if any(k in lbl.lower() for k in ['trigger', 'bumper', 'stick', 'd-pad', 'guide', 'face', 'button']):
        print(f"[{lbl}] id={eid}, tag={tag}, attribs={elem.attrib}")
