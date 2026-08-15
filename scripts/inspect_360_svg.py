import xml.etree.ElementTree as ET
import os

svg_360 = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox 360 Controller Images\Default Theme\Theme SVG\Xbox 360 VSCView - Black.svg"
tree = ET.parse(svg_360)
root = tree.getroot()

print("Root viewBox:", root.attrib.get('viewBox'))
print("Root width/height:", root.attrib.get('width'), root.attrib.get('height'))

for child in root:
    tag = child.tag.split('}')[-1]
    id_val = child.attrib.get('id', '')
    lbl = child.attrib.get('{http://www.inkscape.org/namespaces/inkscape}label', '')
    print(f"Child: tag={tag:10s} id={id_val:20s} label={lbl}")
    for sub in child:
        sub_tag = sub.tag.split('}')[-1]
        sub_id = sub.attrib.get('id', '')
        sub_lbl = sub.attrib.get('{http://www.inkscape.org/namespaces/inkscape}label', '')
        print(f"   Sub: tag={sub_tag:10s} id={sub_id:20s} label={sub_lbl}")
