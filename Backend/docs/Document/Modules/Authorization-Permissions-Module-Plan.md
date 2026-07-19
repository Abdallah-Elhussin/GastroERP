# وحدة إدارة الصلاحيات (Authorization & Permissions) — خطة التنفيذ

> مصدر المواصفات: تعليمات المنتج + شاشات النظام القديم (أدوار / مستخدمين).  
> **قرار:** توسيع **Identity** الحالي — لا إنشاء وحدة Authorization موازية تملك Role/User منفصلة.

---

## الوضع الحالي (ملخص)

| القدرة | الحالة |
|--------|--------|
| Roles / Permissions / RolePermission API | موجودة |
| Users UI (`/finance/users`) | موجودة (ترميز المستخدمين) |
| Roles + Matrix UI | ✅ `/finance/roles` |
| User Permissions Matrix UI | ✅ `/finance/user-permissions` |
| Permission claims في JWT و `/me` | ✅ عبر `IEffectivePermissionService` |
| UserPermission overrides | ✅ Allow / Deny |
| Scopes (شركة/مستودع/مركز تكلفة) | UserBranch فقط |
| Audit DB / Sessions admin / LoginHistory | غير مكتملة |

---

## مراحل التنفيذ

### Phase A — واجهة الأدوار والمصفوفة ✅
- [x] `GetRolePermissions` + `ReplaceRolePermissions` + `CopyRole`
- [x] السماح بتعديل صلاحيات أدوار النظام (مع منع حذف/تغيير اسم النظامي)
- [x] شاشة RTL: قائمة أدوار + مصفوفة (عرض/إضافة/تعديل/حذف/ترحيل/طباعة/اعتماد)
- [x] أزرار: حفظ / كامل / عرض فقط / مسح / إضافة / تعديل / نسخ
- [x] تبويب مشترك مع صلاحيات المستخدم وترميز المستخدمين

### Phase B — صلاحيات فعّالة في التشغيل ✅
- [x] تحميل صلاحيات الأدوار في JWT و `CurrentUserDto.Permissions`
- [x] صيغة الفعّالة: `(Role ∪ UserAllow) − UserDeny`
- [x] Cache قصير العمر (دقيقتان) + إبطال عند تغيير الدور/الاستثناءات
- [x] claims من نوع `Permission` عبر `ClaimsFactory`

### Phase C — شاشة صلاحيات المستخدم ✅
- [x] كيان `UserPermission` (Allow / Deny) + هجرة `AddUserPermissions`
- [x] API: `GET/PUT .../users/{id}/permissions` + `DELETE .../overrides`
- [x] تهيئة من الدور (مسح الاستثناءات) + استثناءات Allow/Deny
- [x] واجهة فلترة مستخدم / فرع + عرض الدور + مصفوفة فعّالة

### Phase D — النطاقات ومجموعات الصلاحيات
- [ ] PermissionCategory / Group seed وربط الكتالوج
- [ ] نطاقات شركة / فرع / مستودع / مركز تكلفة لدور ومستخدم
- [ ] Data Permissions

### Phase E — أمن الدخول والتدقيق
- [ ] LoginHistory، جلسات (عرض/إنهاء)، AuditLog مستمرة
- [ ] 2FA / سياسات كلمة المرور (حسب المنصة)

---

## مسارات API المستخدمة

| Method | Path |
|--------|------|
| GET | `/api/v1/identity/roles` |
| POST | `/api/v1/identity/roles` |
| PUT | `/api/v1/identity/roles/{id}` |
| POST | `/api/v1/identity/roles/{id}/copy` |
| GET | `/api/v1/identity/roles/permissions` |
| GET | `/api/v1/identity/roles/{id}/permissions` |
| PUT | `/api/v1/identity/roles/{id}/permissions` |
| GET | `/api/v1/identity/users/{id}/permissions` |
| PUT | `/api/v1/identity/users/{id}/permissions` |
| DELETE | `/api/v1/identity/users/{id}/permissions/overrides` |

---

## قواعد UI الحالية

- أعمدة المصفوفة تُطابق أفعال Permission Keys (`*.View`, `*.Create`, …) وليست FormId.
- الخلايا الفارغة (—) تعني عدم وجود صلاحية مطابقة لهذا الفعل في الوحدة.
- حفظ مصفوفة الدور = استبدال كامل لصلاحيات الدور (`Replace`).
- حفظ مصفوفة المستخدم = desired effective set → يُحوَّل إلى Allow/Deny بالنسبة لخط أساس الدور.

تاريخ التثبيت: 2026-07-19  
آخر تحديث: 2026-07-19 (Phase B + C)
