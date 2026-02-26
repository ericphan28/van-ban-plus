"""
Tạo favicon.ico cho VanBanPlus website
Chữ "V" trắng trên nền gradient xanh, giống logo trên Navbar
"""
from PIL import Image, ImageDraw, ImageFont
import os

def create_favicon():
    sizes = [16, 32, 48, 64, 128, 256]
    images = []
    
    for size in sizes:
        img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        
        # Gradient background (primary-500 to primary-700: #3b82f6 to #1d4ed8)
        for y in range(size):
            ratio = y / size
            r = int(59 + (29 - 59) * ratio)
            g = int(130 + (78 - 130) * ratio)
            b = int(246 + (216 - 246) * ratio)
            for x in range(size):
                # Rounded corners
                corner_radius = size // 5
                in_rect = True
                # Check corners
                for cx, cy in [(corner_radius, corner_radius), 
                               (size - corner_radius - 1, corner_radius),
                               (corner_radius, size - corner_radius - 1), 
                               (size - corner_radius - 1, size - corner_radius - 1)]:
                    if (x < corner_radius or x > size - corner_radius - 1) and \
                       (y < corner_radius or y > size - corner_radius - 1):
                        dx = x - cx
                        dy = y - cy
                        if dx * dx + dy * dy > corner_radius * corner_radius:
                            in_rect = False
                            break
                if in_rect:
                    img.putpixel((x, y), (r, g, b, 255))
        
        # Draw "V" letter
        try:
            font_size = int(size * 0.65)
            font = ImageFont.truetype("C:/Windows/Fonts/arialbd.ttf", font_size)
        except:
            font = ImageFont.load_default()
        
        bbox = draw.textbbox((0, 0), "V", font=font)
        text_w = bbox[2] - bbox[0]
        text_h = bbox[3] - bbox[1]
        x = (size - text_w) // 2
        y = (size - text_h) // 2 - bbox[1]
        
        draw.text((x, y), "V", fill=(255, 255, 255, 255), font=font)
        images.append(img)
    
    # Save as .ico
    output_path = os.path.join("D:\\AIVanBanCaNhan", "vanbanplus-api", "public", "favicon.ico")
    # ICO format: save the largest, include all sizes
    images[-1].save(
        output_path, 
        format='ICO', 
        sizes=[(s, s) for s in sizes],
        append_images=images[:-1]
    )
    print(f"✅ Favicon created: {output_path}")
    print(f"   Sizes: {', '.join(f'{s}x{s}' for s in sizes)}")
    print(f"   File size: {os.path.getsize(output_path) / 1024:.1f} KB")

if __name__ == "__main__":
    create_favicon()
