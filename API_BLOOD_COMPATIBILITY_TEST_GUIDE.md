# Blood Compatibility API Test Guide

D??i ?ây là h??ng d?n test các API m?i ???c t?o cho ch?c n?ng tra c?u nhóm máu t??ng thích:

## 1. API Tra c?u nhóm máu t??ng thích cho máu toàn ph?n

**GET** `/api/BloodCompatibility/whole-blood/{recipientBloodGroupId}`

**Mô t?:** L?y danh sách các nhóm máu có th? hi?n cho ng??i nh?n v?i nhóm máu c? th? (truy?n máu toàn ph?n)

**Ví d?:**
```
GET /api/BloodCompatibility/whole-blood/A1B2C3D4-E5F6-7890-ABCD-EF1234567890
```

**Response:**
```json
{
    "success": true,
    "data": [
        {
            "id": "A1B2C3D4-E5F6-7890-ABCD-EF1234567890",
            "groupName": "A+",
            "description": "Blood type A positive"
        },
        {
            "id": "B2C3D4E5-F6G7-8901-BCDE-F23456789012",
            "groupName": "A-",
            "description": "Blood type A negative"
        }
    ],
    "message": "Found 4 compatible blood groups for whole blood transfusion to recipient with blood group A+",
    "statusCode": 200
}
```

## 2. API Tra c?u nhóm máu t??ng thích cho thành ph?n máu c? th?

**GET** `/api/BloodCompatibility/component/{recipientBloodGroupId}/{componentTypeId}`

**Mô t?:** L?y danh sách các nhóm máu có th? hi?n thành ph?n máu c? th? cho ng??i nh?n

**Ví d?:**
```
GET /api/BloodCompatibility/component/A1B2C3D4-E5F6-7890-ABCD-EF1234567890/C3D4E5F6-G7H8-9012-CDEF-345678901234
```

**Response:**
```json
{
    "success": true,
    "data": [
        {
            "id": "A1B2C3D4-E5F6-7890-ABCD-EF1234567890",
            "groupName": "A+",
            "description": "Blood type A positive"
        }
    ],
    "message": "Found 2 compatible blood groups for Plasma transfusion to recipient with blood group A+",
    "statusCode": 200
}
```

## 3. API L?y ma tr?n t??ng thích ??y ??

**GET** `/api/BloodCompatibility/matrix`

**Mô t?:** L?y ma tr?n t??ng thích ??y ?? c?a t?t c? nhóm máu và thành ph?n máu

**Ví d?:**
```
GET /api/BloodCompatibility/matrix
```

**Response:**
```json
{
    "success": true,
    "data": [
        {
            "bloodGroupId": "A1B2C3D4-E5F6-7890-ABCD-EF1234567890",
            "bloodGroupName": "A+",
            "canDonateTo": [
                {
                    "bloodGroupId": "D4E5F6G7-H8I9-0123-DEFG-456789012345",
                    "bloodGroupName": "A+"
                },
                {
                    "bloodGroupId": "E5F6G7H8-I9J0-1234-EFGH-567890123456",
                    "bloodGroupName": "AB+"
                }
            ],
            "canReceiveFrom": [
                {
                    "bloodGroupId": "A1B2C3D4-E5F6-7890-ABCD-EF1234567890",
                    "bloodGroupName": "A+"
                },
                {
                    "bloodGroupId": "B2C3D4E5-F6G7-8901-BCDE-F23456789012",
                    "bloodGroupName": "A-"
                }
            ],
            "componentCompatibility": [
                {
                    "componentTypeId": "C3D4E5F6-G7H8-9012-CDEF-345678901234",
                    "componentTypeName": "Red Blood Cells",
                    "compatibleDonors": [
                        {
                            "bloodGroupId": "A1B2C3D4-E5F6-7890-ABCD-EF1234567890",
                            "bloodGroupName": "A+"
                        }
                    ]
                }
            ]
        }
    ],
    "message": "Retrieved blood group compatibility matrix with 8 blood groups",
    "statusCode": 200
}
```

## 4. API Tìm ki?m v?i tùy ch?n

**POST** `/api/BloodCompatibility/search`

**Mô t?:** Tìm ki?m nhóm máu t??ng thích v?i các tùy ch?n

**Request Body:**
```json
{
    "recipientBloodGroupId": "A1B2C3D4-E5F6-7890-ABCD-EF1234567890",
    "componentTypeId": "C3D4E5F6-G7H8-9012-CDEF-345678901234",
    "isWholeBloodSearch": false
}
```

**Response:** T??ng t? nh? API component compatibility

## Quy t?c t??ng thích nhóm máu

### Máu toàn ph?n:
- **O-**: Có th? nh?n t? O-
- **O+**: Có th? nh?n t? O+, O-
- **A-**: Có th? nh?n t? A-, O-
- **A+**: Có th? nh?n t? A+, A-, O+, O-
- **B-**: Có th? nh?n t? B-, O-
- **B+**: Có th? nh?n t? B+, B-, O+, O-
- **AB-**: Có th? nh?n t? A-, B-, AB-, O-
- **AB+**: Có th? nh?n t? t?t c? (ng??i nh?n toàn n?ng)

### Huy?t t??ng:
- Tuân theo quy t?c ng??c v?i máu toàn ph?n
- **AB+**: Có th? nh?n t? AB+, AB-
- **AB-**: Có th? nh?n t? AB-
- **A+**: Có th? nh?n t? A+, A-, AB+, AB-
- **A-**: Có th? nh?n t? A-, AB-
- **B+**: Có th? nh?n t? B+, B-, AB+, AB-
- **B-**: Có th? nh?n t? B-, AB-
- **O+**: Có th? nh?n t? t?t c?
- **O-**: Có th? nh?n t? O-, A-, B-, AB-

### H?ng c?u:
- Tuân theo quy t?c t??ng t? máu toàn ph?n

### Ti?u c?u:
- Thích h?p nh?t khi cùng nhóm ABO
- Có th? truy?n không ??ng nhóm trong tr??ng h?p kh?n c?p

## L?i có th? g?p

### 404 - Not Found
```json
{
    "success": false,
    "message": "Recipient blood group not found",
    "statusCode": 404
}
```

### 400 - Bad Request
```json
{
    "success": false,
    "message": "Component type not found",
    "statusCode": 400
}
```

### 500 - Internal Server Error
```json
{
    "success": false,
    "message": "Error occurred while getting compatible blood groups",
    "statusCode": 500
}
```