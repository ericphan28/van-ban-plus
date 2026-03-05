# Hướng dẫn: Download Album Ảnh từ Cổng Thông Tin Xã Gia Kiệm

> **Mục đích**: Tài liệu này hướng dẫn Copilot (hoặc developer) cách download toàn bộ album ảnh từ website [xagiakiem.gov.vn](https://www.xagiakiem.gov.vn) để xây dựng tính năng tương tự trong một project Next.js + Supabase khác.

---

## 1. Tổng Quan Kiến Trúc Website Nguồn

### 1.1 Stack công nghệ
- **Frontend**: Next.js (App Router)
- **Backend/Database**: Supabase (PostgreSQL)
- **Image Storage**: Supabase Storage — bucket `album-images`
- **Supabase Project ID**: `mbihsygtjfxxjdgtmwyc`
- **Domain**: `www.xagiakiem.gov.vn`

### 1.2 Cấu trúc dữ liệu

Website có 3 entity chính liên quan đến album:

```
organizations (tổ chức)
  └── albums (album ảnh)
       └── album_images (ảnh trong album)
```

---

## 2. API Endpoints (Public, không cần auth)

### 2.1 Danh sách Albums

```
GET https://www.xagiakiem.gov.vn/api/albums
GET https://www.xagiakiem.gov.vn/api/albums?page=2
```

**Query params:**
| Param | Type | Default | Mô tả |
|-------|------|---------|-------|
| `page` | number | 1 | Trang hiện tại |
| `limit` | number | 20 | Số album mỗi trang |

**Response format:**
```json
{
  "items": [
    {
      "id": "ae1c3b1a-3d2c-48c2-ab60-b2222616d6c4",
      "title": "Hội nghị bình cử thực hiện nghĩa vụ quân sự năm 2026",
      "description": "Chiều 14-1, Hội đồng nghĩa vụ quân sự xã Gia Kiệm...",
      "created_at": "2026-01-15T02:48:58.734892+00:00",
      "cover_url": "https://mbihsygtjfxxjdgtmwyc.supabase.co/storage/v1/object/public/album-images/albums/ae1c3b1a-.../1768445500289-1.jpg",
      "image_count": 16,
      "event_title": null,
      "event_date": null,
      "organization_name": "Ban Chỉ huy Quân sự xã Gia Kiệm",
      "organization_id": "2ee53d32-5587-43d2-bf5c-d8a8d599cc83"
    }
  ],
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 62,
    "totalPages": 4,
    "hasNext": true,
    "hasPrev": false
  }
}
```

### 2.2 Chi tiết Album (bao gồm danh sách ảnh)

```
GET https://www.xagiakiem.gov.vn/api/albums/{album_id}
```

**Response format:**
```json
{
  "id": "ae1c3b1a-3d2c-48c2-ab60-b2222616d6c4",
  "title": "Hội nghị bình cử thực hiện nghĩa vụ quân sự năm 2026",
  "description": "Chiều 14-1, Hội đồng nghĩa vụ quân sự...",
  "created_at": "2026-01-15T02:48:58.734892+00:00",
  "cover_url": "https://...jpg",
  "event_title": null,
  "event_date": null,
  "organization_name": "Ban Chỉ huy Quân sự xã Gia Kiệm",
  "image_count": 16,
  "images": [
    {
      "id": "3687bdfb-7948-498c-999b-b08e26fbf7b7",
      "image_url": "https://mbihsygtjfxxjdgtmwyc.supabase.co/storage/v1/object/public/album-images/albums/ae1c3b1a-.../1768445500289-1.jpg",
      "title": "1",
      "display_order": 1,
      "is_cover": false,
      "size_bytes": 261347
    },
    {
      "id": "640bc3d9-9387-4ae1-a822-15109161d780",
      "image_url": "https://...2.jpg",
      "title": "2",
      "display_order": 2,
      "is_cover": false,
      "size_bytes": 261316
    }
  ]
}
```

### 2.3 Chỉ lấy danh sách ảnh của Album

```
GET https://www.xagiakiem.gov.vn/api/albums/{album_id}/images
```

**Response format:**
```json
{
  "images": [
    {
      "id": "3afa003a-4a7e-4451-9917-698c362465ef",
      "image_url": "https://mbihsygtjfxxjdgtmwyc.supabase.co/storage/v1/object/public/album-images/albums/{album_id}/{timestamp}-{filename}.jpg",
      "title": "31",
      "display_order": 1,
      "is_cover": false
    }
  ]
}
```

---

## 3. Cấu Trúc URL Ảnh (Supabase Storage)

### 3.1 Pattern URL gốc (original)

```
https://mbihsygtjfxxjdgtmwyc.supabase.co/storage/v1/object/public/album-images/albums/{album_id}/{timestamp}-{filename}.{ext}
```

**Ví dụ:**
```
https://mbihsygtjfxxjdgtmwyc.supabase.co/storage/v1/object/public/album-images/albums/ae1c3b1a-3d2c-48c2-ab60-b2222616d6c4/1768445500289-1.jpg
```

### 3.2 Pattern URL với Next.js Image Optimization

Website dùng `next/image` nên trên frontend, ảnh có thêm query params:
```
?w=256&q=75    // thumbnail trong album grid
?w=640&q=75    // card preview
?w=828&q=75    // hero/cover image
```

### 3.3 Phân tích filename

```
{timestamp}-{original_filename_or_index}.{ext}
```

- `timestamp`: Unix timestamp milliseconds (thời điểm upload)
- `original_filename`: tên file gốc hoặc số thứ tự
- `ext`: jpg, jpeg, png

**Ví dụ:**
- `1768445500289-1.jpg` → upload lúc timestamp 1768445500289, file gốc `1.jpg`
- `1772546897817-644471210_1334418148727954_6360143781211106368_n.jpg` → file từ Facebook

### 3.4 Hai loại album_id trong path

1. **UUID** (album mới): `ae1c3b1a-3d2c-48c2-ab60-b2222616d6c4`
2. **Slug** (album cũ, migrated): `xet-duyet-tieu-chuan-san-sang-nhap-ngu-va-so-tuyet-suc-khoe`

---

## 4. Database Schema (Supabase PostgreSQL)

Dựa trên phân tích API responses, schema Supabase gồm các bảng sau:

### 4.1 Bảng `organizations`

```sql
CREATE TABLE organizations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name TEXT NOT NULL,
    -- ... các field khác
);
```

**Dữ liệu mẫu:**
| id | name |
|----|------|
| `409f045f-add9-4859-89b3-2ce5174ca6de` | Ủy ban Nhân dân xã Gia Kiệm |
| `8d1e5ad0-a07e-40df-bf3b-3dbd1663095b` | Đảng ủy xã Gia Kiệm |
| `2ee53d32-5587-43d2-bf5c-d8a8d599cc83` | Ban Chỉ huy Quân sự xã Gia Kiệm |
| `c05929fc-21df-426d-b06b-4daa901e4d20` | Công an xã Gia Kiệm |
| `a78232b6-3899-4169-8bbd-ef845ac3e7dc` | Trạm Y tế xã Gia Kiệm |

### 4.2 Bảng `albums`

```sql
CREATE TABLE albums (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title TEXT NOT NULL,
    description TEXT DEFAULT '',
    cover_url TEXT,                          -- URL ảnh bìa
    event_title TEXT,                        -- Tên sự kiện (có thể null)
    event_date DATE,                         -- Ngày sự kiện (có thể null)
    organization_id UUID REFERENCES organizations(id),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);
```

**Lưu ý quan trọng:**
- `cover_url` là URL đầy đủ Supabase Storage (không phải relative path)
- `event_title` và `event_date` là OPTIONAL — nhiều album không có
- `description` có thể rất dài (nội dung bài viết)
- Tổng: **62 albums** (tính đến thời điểm khảo sát)

### 4.3 Bảng `album_images`

```sql
CREATE TABLE album_images (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    album_id UUID REFERENCES albums(id) ON DELETE CASCADE,
    image_url TEXT NOT NULL,                 -- Full Supabase Storage URL
    title TEXT DEFAULT '',                   -- Thường là số thứ tự: "1", "2", ...
    display_order INT DEFAULT 0,             -- Thứ tự hiển thị
    is_cover BOOLEAN DEFAULT FALSE,
    size_bytes BIGINT DEFAULT 0,             -- Kích thước file (bytes)
    created_at TIMESTAMPTZ DEFAULT NOW()
);
```

### 4.4 Supabase Storage Bucket

```
Bucket name: album-images
Public: YES (public access, không cần auth để đọc)
Structure:
  album-images/
    albums/
      {album_id}/
        {timestamp}-{filename}.jpg
        {timestamp}-{filename}.jpg
        ...
```

---

## 5. Script Download Toàn Bộ Albums

### 5.1 Script Node.js — Download metadata + ảnh

```typescript
// scripts/download-albums.ts
import fs from 'fs';
import path from 'path';
import https from 'https';

const BASE_URL = 'https://www.xagiakiem.gov.vn/api/albums';
const OUTPUT_DIR = './downloaded-albums';

interface AlbumImage {
  id: string;
  image_url: string;
  title: string;
  display_order: number;
  is_cover: boolean;
  size_bytes?: number;
}

interface Album {
  id: string;
  title: string;
  description: string;
  created_at: string;
  cover_url: string | null;
  image_count: number;
  event_title: string | null;
  event_date: string | null;
  organization_name: string;
  organization_id: string;
  images?: AlbumImage[];
}

interface AlbumsResponse {
  items: Album[];
  pagination: {
    page: number;
    limit: number;
    total: number;
    totalPages: number;
    hasNext: boolean;
    hasPrev: boolean;
  };
}

// 1. Fetch tất cả albums (pagination)
async function fetchAllAlbums(): Promise<Album[]> {
  const allAlbums: Album[] = [];
  let page = 1;
  let hasNext = true;

  while (hasNext) {
    console.log(`📄 Fetching page ${page}...`);
    const res = await fetch(`${BASE_URL}?page=${page}&limit=20`);
    const data: AlbumsResponse = await res.json();
    
    allAlbums.push(...data.items);
    hasNext = data.pagination.hasNext;
    page++;
    
    // Rate limiting
    await new Promise(r => setTimeout(r, 500));
  }

  console.log(`✅ Total albums: ${allAlbums.length}`);
  return allAlbums;
}

// 2. Fetch chi tiết album (bao gồm images)
async function fetchAlbumDetail(albumId: string): Promise<Album> {
  const res = await fetch(`${BASE_URL}/${albumId}`);
  return res.json();
}

// 3. Download ảnh
async function downloadImage(url: string, filepath: string): Promise<void> {
  return new Promise((resolve, reject) => {
    const file = fs.createWriteStream(filepath);
    https.get(url, (response) => {
      // Handle redirects
      if (response.statusCode === 301 || response.statusCode === 302) {
        const redirectUrl = response.headers.location;
        if (redirectUrl) {
          https.get(redirectUrl, (res2) => {
            res2.pipe(file);
            file.on('finish', () => { file.close(); resolve(); });
          });
        }
        return;
      }
      response.pipe(file);
      file.on('finish', () => { file.close(); resolve(); });
    }).on('error', (err) => {
      fs.unlink(filepath, () => {});
      reject(err);
    });
  });
}

// 4. Main
async function main() {
  // Tạo thư mục output
  fs.mkdirSync(OUTPUT_DIR, { recursive: true });

  // Fetch all albums
  const albums = await fetchAllAlbums();
  
  // Lưu metadata
  fs.writeFileSync(
    path.join(OUTPUT_DIR, 'albums-metadata.json'),
    JSON.stringify(albums, null, 2),
    'utf-8'
  );

  // Download từng album
  for (const album of albums) {
    console.log(`\n📁 Album: ${album.title} (${album.image_count} ảnh)`);
    
    // Tạo thư mục cho album
    const albumDir = path.join(OUTPUT_DIR, album.id);
    fs.mkdirSync(albumDir, { recursive: true });
    
    // Fetch album detail (có images)
    const detail = await fetchAlbumDetail(album.id);
    
    // Lưu metadata album
    fs.writeFileSync(
      path.join(albumDir, 'metadata.json'),
      JSON.stringify(detail, null, 2),
      'utf-8'
    );

    // Download từng ảnh
    if (detail.images && detail.images.length > 0) {
      for (const img of detail.images) {
        const ext = path.extname(new URL(img.image_url).pathname) || '.jpg';
        const filename = `${img.display_order.toString().padStart(3, '0')}_${img.title}${ext}`;
        const filepath = path.join(albumDir, filename);

        if (fs.existsSync(filepath)) {
          console.log(`  ⏭ Skip: ${filename}`);
          continue;
        }

        try {
          await downloadImage(img.image_url, filepath);
          console.log(`  ✅ ${filename}`);
        } catch (err) {
          console.error(`  ❌ ${filename}: ${err}`);
        }

        // Rate limiting
        await new Promise(r => setTimeout(r, 200));
      }
    }
    
    // Rate limiting between albums
    await new Promise(r => setTimeout(r, 300));
  }

  console.log('\n🎉 Done!');
}

main().catch(console.error);
```

### 5.2 Chạy script

```bash
npx tsx scripts/download-albums.ts
```

---

## 6. Hướng Dẫn Xây Dựng Tính Năng Tương Tự (Next.js + Supabase)

### 6.1 Setup Supabase

#### Tạo bảng

```sql
-- 1. Organizations
CREATE TABLE organizations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name TEXT NOT NULL,
    slug TEXT UNIQUE,
    description TEXT DEFAULT '',
    logo_url TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- 2. Albums
CREATE TABLE albums (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title TEXT NOT NULL,
    slug TEXT UNIQUE,                        -- URL-friendly title
    description TEXT DEFAULT '',
    cover_url TEXT,
    event_title TEXT,
    event_date DATE,
    organization_id UUID REFERENCES organizations(id),
    is_published BOOLEAN DEFAULT TRUE,
    is_pinned BOOLEAN DEFAULT FALSE,         -- Ghim lên đầu
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- Index cho query nhanh
CREATE INDEX idx_albums_org ON albums(organization_id);
CREATE INDEX idx_albums_created ON albums(created_at DESC);
CREATE INDEX idx_albums_published ON albums(is_published) WHERE is_published = TRUE;

-- 3. Album Images
CREATE TABLE album_images (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    album_id UUID REFERENCES albums(id) ON DELETE CASCADE NOT NULL,
    image_url TEXT NOT NULL,
    title TEXT DEFAULT '',
    caption TEXT DEFAULT '',                  -- Mô tả ảnh
    display_order INT DEFAULT 0,
    is_cover BOOLEAN DEFAULT FALSE,
    size_bytes BIGINT DEFAULT 0,
    width INT,                               -- Chiều rộng gốc (px)
    height INT,                              -- Chiều cao gốc (px)
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE INDEX idx_album_images_album ON album_images(album_id);
CREATE INDEX idx_album_images_order ON album_images(album_id, display_order);

-- 4. RLS Policies
ALTER TABLE organizations ENABLE ROW LEVEL SECURITY;
ALTER TABLE albums ENABLE ROW LEVEL SECURITY;
ALTER TABLE album_images ENABLE ROW LEVEL SECURITY;

-- Public read
CREATE POLICY "Public read organizations" ON organizations FOR SELECT USING (true);
CREATE POLICY "Public read albums" ON albums FOR SELECT USING (is_published = true);
CREATE POLICY "Public read album_images" ON album_images FOR SELECT USING (true);

-- Service role full access (cho admin API)
CREATE POLICY "Service full access organizations" ON organizations FOR ALL TO service_role USING (true);
CREATE POLICY "Service full access albums" ON albums FOR ALL TO service_role USING (true);
CREATE POLICY "Service full access album_images" ON album_images FOR ALL TO service_role USING (true);
```

#### Tạo Storage Bucket

```sql
-- Trong Supabase Dashboard → Storage → New Bucket
-- Name: album-images
-- Public: ON
-- File size limit: 10MB
-- Allowed MIME types: image/jpeg, image/png, image/webp
```

Storage policy (cho phép public read, admin upload):
```sql
-- Policy cho bucket album-images
CREATE POLICY "Public read album images"
ON storage.objects FOR SELECT
USING (bucket_id = 'album-images');

CREATE POLICY "Admin upload album images"
ON storage.objects FOR INSERT
TO authenticated
WITH CHECK (bucket_id = 'album-images');

CREATE POLICY "Admin delete album images"
ON storage.objects FOR DELETE
TO authenticated
USING (bucket_id = 'album-images');
```

### 6.2 Supabase Client Setup

```typescript
// src/lib/supabase.ts
import { createClient } from '@supabase/supabase-js';

const supabaseUrl = process.env.NEXT_PUBLIC_SUPABASE_URL!;
const supabaseAnonKey = process.env.NEXT_PUBLIC_SUPABASE_ANON_KEY!;
const supabaseServiceKey = process.env.SUPABASE_SERVICE_ROLE_KEY!;

// Client-side (anon key, RLS enforced)
export const supabase = createClient(supabaseUrl, supabaseAnonKey);

// Server-side (service role, bypass RLS) — CHỈ dùng trong API routes
export const supabaseAdmin = createClient(supabaseUrl, supabaseServiceKey);
```

### 6.3 API Routes (Next.js App Router)

#### GET /api/albums — Danh sách albums

```typescript
// src/app/api/albums/route.ts
import { NextRequest, NextResponse } from 'next/server';
import { supabaseAdmin } from '@/lib/supabase';

export async function GET(request: NextRequest) {
  const { searchParams } = new URL(request.url);
  const page = parseInt(searchParams.get('page') || '1');
  const limit = parseInt(searchParams.get('limit') || '20');
  const offset = (page - 1) * limit;

  // Count total
  const { count } = await supabaseAdmin
    .from('albums')
    .select('*', { count: 'exact', head: true })
    .eq('is_published', true);

  // Fetch albums with organization name
  const { data: albums, error } = await supabaseAdmin
    .from('albums')
    .select(`
      id, title, description, created_at, cover_url,
      event_title, event_date,
      organizations!inner(id, name)
    `)
    .eq('is_published', true)
    .order('is_pinned', { ascending: false })
    .order('created_at', { ascending: false })
    .range(offset, offset + limit - 1);

  if (error) {
    return NextResponse.json({ error: error.message }, { status: 500 });
  }

  // Count images per album
  const albumIds = albums?.map(a => a.id) || [];
  const { data: imageCounts } = await supabaseAdmin
    .rpc('get_album_image_counts', { album_ids: albumIds });

  // Transform response
  const items = albums?.map(album => {
    const org = album.organizations as any;
    const imgCount = imageCounts?.find((c: any) => c.album_id === album.id);
    return {
      id: album.id,
      title: album.title,
      description: album.description,
      created_at: album.created_at,
      cover_url: album.cover_url,
      image_count: imgCount?.count || 0,
      event_title: album.event_title,
      event_date: album.event_date,
      organization_name: org?.name || null,
      organization_id: org?.id || null,
    };
  });

  const total = count || 0;
  const totalPages = Math.ceil(total / limit);

  return NextResponse.json({
    items,
    pagination: {
      page,
      limit,
      total,
      totalPages,
      hasNext: page < totalPages,
      hasPrev: page > 1,
    },
  });
}
```

#### GET /api/albums/[id] — Chi tiết album

```typescript
// src/app/api/albums/[id]/route.ts
import { NextRequest, NextResponse } from 'next/server';
import { supabaseAdmin } from '@/lib/supabase';

export async function GET(
  request: NextRequest,
  { params }: { params: { id: string } }
) {
  const { id } = params;

  // Fetch album with organization
  const { data: album, error } = await supabaseAdmin
    .from('albums')
    .select(`
      id, title, description, created_at, cover_url,
      event_title, event_date,
      organizations(id, name)
    `)
    .eq('id', id)
    .eq('is_published', true)
    .single();

  if (error || !album) {
    return NextResponse.json({ error: 'Album not found' }, { status: 404 });
  }

  // Fetch images
  const { data: images } = await supabaseAdmin
    .from('album_images')
    .select('id, image_url, title, display_order, is_cover, size_bytes')
    .eq('album_id', id)
    .order('display_order', { ascending: true });

  const org = album.organizations as any;

  return NextResponse.json({
    id: album.id,
    title: album.title,
    description: album.description,
    created_at: album.created_at,
    cover_url: album.cover_url,
    event_title: album.event_title,
    event_date: album.event_date,
    organization_name: org?.name || null,
    image_count: images?.length || 0,
    images: images || [],
  });
}
```

#### GET /api/albums/[id]/images — Chỉ lấy ảnh

```typescript
// src/app/api/albums/[id]/images/route.ts
import { NextRequest, NextResponse } from 'next/server';
import { supabaseAdmin } from '@/lib/supabase';

export async function GET(
  request: NextRequest,
  { params }: { params: { id: string } }
) {
  const { data: images, error } = await supabaseAdmin
    .from('album_images')
    .select('id, image_url, title, display_order, is_cover')
    .eq('album_id', params.id)
    .order('display_order', { ascending: true });

  if (error) {
    return NextResponse.json({ error: error.message }, { status: 500 });
  }

  return NextResponse.json({ images: images || [] });
}
```

### 6.4 Upload Ảnh vào Album (Admin API)

```typescript
// src/app/api/admin/albums/[id]/upload/route.ts
import { NextRequest, NextResponse } from 'next/server';
import { supabaseAdmin } from '@/lib/supabase';

export async function POST(
  request: NextRequest,
  { params }: { params: { id: string } }
) {
  const albumId = params.id;
  const formData = await request.formData();
  const files = formData.getAll('files') as File[];

  if (!files.length) {
    return NextResponse.json({ error: 'No files provided' }, { status: 400 });
  }

  // Lấy display_order hiện tại cao nhất
  const { data: lastImage } = await supabaseAdmin
    .from('album_images')
    .select('display_order')
    .eq('album_id', albumId)
    .order('display_order', { ascending: false })
    .limit(1)
    .single();

  let nextOrder = (lastImage?.display_order || 0) + 1;
  const uploadedImages = [];

  for (const file of files) {
    const timestamp = Date.now();
    const safeName = file.name.replace(/[^a-zA-Z0-9._-]/g, '-');
    const storagePath = `albums/${albumId}/${timestamp}-${safeName}`;

    // Upload to Supabase Storage
    const buffer = Buffer.from(await file.arrayBuffer());
    const { data: uploadData, error: uploadError } = await supabaseAdmin
      .storage
      .from('album-images')
      .upload(storagePath, buffer, {
        contentType: file.type,
        upsert: false,
      });

    if (uploadError) {
      console.error('Upload error:', uploadError);
      continue;
    }

    // Lấy public URL
    const { data: urlData } = supabaseAdmin
      .storage
      .from('album-images')
      .getPublicUrl(storagePath);

    // Insert vào album_images
    const { data: imageRecord, error: insertError } = await supabaseAdmin
      .from('album_images')
      .insert({
        album_id: albumId,
        image_url: urlData.publicUrl,
        title: nextOrder.toString(),
        display_order: nextOrder,
        is_cover: nextOrder === 1,
        size_bytes: file.size,
      })
      .select()
      .single();

    if (!insertError && imageRecord) {
      uploadedImages.push(imageRecord);
    }

    nextOrder++;
  }

  // Cập nhật cover_url nếu chưa có
  if (uploadedImages.length > 0) {
    const { data: album } = await supabaseAdmin
      .from('albums')
      .select('cover_url')
      .eq('id', albumId)
      .single();

    if (!album?.cover_url) {
      await supabaseAdmin
        .from('albums')
        .update({ cover_url: uploadedImages[0].image_url })
        .eq('id', albumId);
    }
  }

  return NextResponse.json({
    uploaded: uploadedImages.length,
    images: uploadedImages,
  });
}
```

### 6.5 Import Albums từ xagiakiem.gov.vn vào project mới

```typescript
// scripts/import-from-xagiakiem.ts
// Script chạy 1 lần để import albums từ xagiakiem.gov.vn vào Supabase project mới

import { createClient } from '@supabase/supabase-js';

const SOURCE_API = 'https://www.xagiakiem.gov.vn/api/albums';

// Supabase project MỚI (destination)
const DEST_SUPABASE_URL = process.env.DEST_SUPABASE_URL!;
const DEST_SUPABASE_KEY = process.env.DEST_SUPABASE_SERVICE_KEY!;
const destSupabase = createClient(DEST_SUPABASE_URL, DEST_SUPABASE_KEY);

async function importAlbums() {
  let page = 1;
  let hasNext = true;

  while (hasNext) {
    const res = await fetch(`${SOURCE_API}?page=${page}&limit=20`);
    const data = await res.json();

    for (const album of data.items) {
      console.log(`📁 Importing: ${album.title}`);

      // 1. Upsert organization
      await destSupabase
        .from('organizations')
        .upsert({
          id: album.organization_id,
          name: album.organization_name,
        }, { onConflict: 'id' });

      // 2. Insert album
      const { error: albumError } = await destSupabase
        .from('albums')
        .upsert({
          id: album.id,
          title: album.title,
          description: album.description,
          cover_url: album.cover_url,
          event_title: album.event_title,
          event_date: album.event_date,
          organization_id: album.organization_id,
          created_at: album.created_at,
        }, { onConflict: 'id' });

      if (albumError) {
        console.error(`  ❌ Album error: ${albumError.message}`);
        continue;
      }

      // 3. Fetch album images
      const detailRes = await fetch(`${SOURCE_API}/${album.id}`);
      const detail = await detailRes.json();

      if (detail.images && detail.images.length > 0) {
        // Cách 1: GIỮ NGUYÊN image_url gốc (hotlink từ Supabase cũ)
        // ⚠️ Không nên dùng lâu dài vì phụ thuộc vào source
        for (const img of detail.images) {
          await destSupabase
            .from('album_images')
            .upsert({
              id: img.id,
              album_id: album.id,
              image_url: img.image_url,
              title: img.title,
              display_order: img.display_order,
              is_cover: img.is_cover,
              size_bytes: img.size_bytes || 0,
            }, { onConflict: 'id' });
        }

        // Cách 2: DOWNLOAD rồi re-upload vào Storage mới
        // (Xem script download ở Section 5.1, sau đó upload lại)
        
        console.log(`  ✅ ${detail.images.length} images imported`);
      }

      await new Promise(r => setTimeout(r, 300));
    }

    hasNext = data.pagination.hasNext;
    page++;
  }

  console.log('🎉 Import complete!');
}

importAlbums().catch(console.error);
```

### 6.6 Helper: Re-upload ảnh vào Storage project mới

```typescript
// scripts/reupload-images.ts
// Download ảnh từ URL cũ → Upload vào Supabase Storage mới

async function reuploadImage(
  sourceUrl: string,
  albumId: string,
  filename: string
): Promise<string> {
  // Download
  const response = await fetch(sourceUrl);
  const buffer = Buffer.from(await response.arrayBuffer());
  const contentType = response.headers.get('content-type') || 'image/jpeg';

  // Upload to new Storage
  const storagePath = `albums/${albumId}/${filename}`;
  const { error } = await destSupabase
    .storage
    .from('album-images')
    .upload(storagePath, buffer, { contentType, upsert: true });

  if (error) throw error;

  // Get new public URL
  const { data } = destSupabase
    .storage
    .from('album-images')
    .getPublicUrl(storagePath);

  return data.publicUrl;
}
```

---

## 7. Frontend Components (Next.js)

### 7.1 next.config.ts — Cấu hình Image domains

```typescript
// next.config.ts
const nextConfig = {
  images: {
    remotePatterns: [
      {
        protocol: 'https',
        hostname: '*.supabase.co',
        pathname: '/storage/v1/object/public/**',
      },
    ],
  },
};

export default nextConfig;
```

### 7.2 Album Grid Component

```tsx
// src/components/AlbumGrid.tsx
'use client';

import Image from 'next/image';
import Link from 'next/link';

interface Album {
  id: string;
  title: string;
  cover_url: string | null;
  image_count: number;
  created_at: string;
  organization_name: string;
}

export function AlbumGrid({ albums }: { albums: Album[] }) {
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {albums.map((album) => (
        <Link key={album.id} href={`/albums/${album.id}`}>
          <div className="group relative rounded-xl overflow-hidden shadow-md hover:shadow-xl transition-shadow">
            {/* Cover Image */}
            <div className="relative aspect-[4/3]">
              {album.cover_url ? (
                <Image
                  src={album.cover_url}
                  alt={album.title}
                  fill
                  className="object-cover group-hover:scale-105 transition-transform"
                  sizes="(max-width: 768px) 100vw, (max-width: 1200px) 50vw, 33vw"
                />
              ) : (
                <div className="w-full h-full bg-gray-200 flex items-center justify-center">
                  <span className="text-gray-400">Chưa có ảnh</span>
                </div>
              )}
              {/* Image count badge */}
              <div className="absolute bottom-2 right-2 bg-black/60 text-white px-2 py-1 rounded-md text-sm">
                📷 {album.image_count} ảnh
              </div>
            </div>
            {/* Info */}
            <div className="p-4">
              <h3 className="font-semibold text-lg line-clamp-2">{album.title}</h3>
              <p className="text-sm text-gray-500 mt-1">{album.organization_name}</p>
              <p className="text-xs text-gray-400 mt-1">
                {new Date(album.created_at).toLocaleDateString('vi-VN')}
              </p>
            </div>
          </div>
        </Link>
      ))}
    </div>
  );
}
```

### 7.3 Album Detail — Lightbox Gallery

```tsx
// src/components/AlbumGallery.tsx
'use client';

import { useState } from 'react';
import Image from 'next/image';

interface AlbumImage {
  id: string;
  image_url: string;
  title: string;
  display_order: number;
}

export function AlbumGallery({ images }: { images: AlbumImage[] }) {
  const [selectedIndex, setSelectedIndex] = useState<number | null>(null);

  return (
    <>
      {/* Grid thumbnails */}
      <div className="grid grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-2">
        {images.map((img, index) => (
          <div
            key={img.id}
            className="relative aspect-square cursor-pointer rounded-lg overflow-hidden"
            onClick={() => setSelectedIndex(index)}
          >
            <Image
              src={img.image_url}
              alt={`Ảnh ${img.display_order}`}
              fill
              className="object-cover hover:opacity-80 transition-opacity"
              sizes="(max-width: 768px) 33vw, (max-width: 1200px) 25vw, 16vw"
            />
            <div className="absolute bottom-1 left-1 bg-black/50 text-white text-xs px-1 rounded">
              {img.display_order}
            </div>
          </div>
        ))}
      </div>

      {/* Lightbox */}
      {selectedIndex !== null && (
        <div
          className="fixed inset-0 z-50 bg-black/90 flex items-center justify-center"
          onClick={() => setSelectedIndex(null)}
        >
          {/* Navigation */}
          <button
            className="absolute left-4 text-white text-4xl z-10"
            onClick={(e) => {
              e.stopPropagation();
              setSelectedIndex(Math.max(0, selectedIndex - 1));
            }}
          >
            ‹
          </button>
          <button
            className="absolute right-4 text-white text-4xl z-10"
            onClick={(e) => {
              e.stopPropagation();
              setSelectedIndex(Math.min(images.length - 1, selectedIndex + 1));
            }}
          >
            ›
          </button>

          {/* Image */}
          <div className="relative w-[90vw] h-[90vh]" onClick={(e) => e.stopPropagation()}>
            <Image
              src={images[selectedIndex].image_url}
              alt={`Ảnh ${images[selectedIndex].display_order}`}
              fill
              className="object-contain"
              sizes="90vw"
              priority
            />
          </div>

          {/* Counter */}
          <div className="absolute bottom-4 text-white text-sm">
            {selectedIndex + 1} / {images.length}
          </div>

          {/* Close button */}
          <button
            className="absolute top-4 right-4 text-white text-3xl"
            onClick={() => setSelectedIndex(null)}
          >
            ✕
          </button>
        </div>
      )}
    </>
  );
}
```

---

## 8. Lưu Ý Quan Trọng

### 8.1 Rate Limiting
- API xagiakiem.gov.vn KHÔNG có rate limit rõ ràng, nhưng hãy thêm delay 200-500ms giữa các request
- Không gọi quá 5 request/giây

### 8.2 Image Sizes
- Ảnh gốc có kích thước từ ~100KB đến ~500KB (đã được resize trước khi upload)
- Tổng dung lượng ước tính: ~62 albums × ~10 ảnh/album × ~300KB = ~180MB

### 8.3 CORS
- API trả JSON nên không có vấn đề CORS
- Supabase Storage có CORS mở (public bucket)
- Nếu download từ browser, dùng server-side proxy

### 8.4 Pagination
- Tổng: **62 albums**, **4 trang** (20 albums/trang)
- Albums sắp xếp theo `created_at DESC` (mới nhất trước)
- Albums có `is_pinned` sẽ được đẩy lên đầu

### 8.5 Encoding
- Title và description dùng UTF-8 (tiếng Việt có dấu)
- Filename trong storage có thể chứa Unicode (đã được encode)

### 8.6 Null handling
- `cover_url`: Có thể null (album chưa có ảnh)
- `event_title`, `event_date`: Thường null cho album thông thường
- `description`: Có thể rỗng ("Album tái tạo từ storage")

---

## 9. Tóm Tắt Nhanh

| Thông tin | Giá trị |
|-----------|---------|
| API endpoint | `https://www.xagiakiem.gov.vn/api/albums` |
| Auth | Không cần (public) |
| Tổng albums | 62 |
| Image storage | Supabase Storage (public bucket `album-images`) |
| Image URL pattern | `https://{project}.supabase.co/storage/v1/object/public/album-images/albums/{album_id}/{timestamp}-{name}.jpg` |
| Response format | JSON |
| Pagination | `?page=1&limit=20` |
| Album detail | `/api/albums/{id}` (trả kèm `images[]`) |
| Album images only | `/api/albums/{id}/images` |

---

*Tài liệu tạo: 2026-03-05 — Phục vụ Copilot hướng dẫn trong session khác*
