# دليل هندسة وتطوير برمجيات GastroERP

# المجلد الأول — التأسيس

## 04-DOMAIN_MODEL_AND_CONTEXTS.md

الإصدار: 1.0

الحالة: مسودة (Draft)

---

# 1. مقدمة
تحدد هذه الوثيقة خريطة السياق (Context Map) والسياقات المقيدة (Bounded Contexts) ونماذج النطاق (Domain Models) الرئيسية لنظام **GastroERP**. تم بناء هذه النماذج بناءً على وثائق تحليل الأعمال وقواعد العمل (01 و 02 و 03).

---

# 2. السياقات المقيدة (Bounded Contexts)

لضمان معمارية نظيفة وقابلة للتوسع (Microservices-Ready)، تم تقسيم النظام إلى السياقات المقيدة التالية:

## 2.1 سياق الهوية والمستأجرين (Identity & Tenant Context)
**المسؤولية:** إدارة المستأجرين (الشركات)، الاشتراكات، الفروع، المستخدمين، والصلاحيات.
- **Aggregates:** 
  - `Tenant` (المستأجر / المؤسسة)
  - `Company` (الشركة)
  - `Branch` (الفرع)
  - `User` (المستخدم)
  - `Role` (الصلاحية)

## 2.2 سياق المبيعات ونقاط البيع (Sales & POS Context)
**المسؤولية:** إدارة الطلبات، الطاولات، وقنوات البيع (محلي، سفري، توصيل). **هذا السياق يعمل بشكل أساسي في وضع Offline-First.**
- **Aggregates:** 
  - `Order` (الطلب / الفاتورة)
  - `Session` (وردية الكاشير)
  - `Table` (الطاولة)
  - `Payment` (الدفعة)

## 2.3 سياق المطبخ (Kitchen Context)
**المسؤولية:** توجيه الطلبات لمحطات التحضير، حساب وقت التجهيز، وإدارة طوابير KDS.
- **Aggregates:** 
  - `KitchenTicket` (تذكرة المطبخ)
  - `PreparationStation` (محطة التحضير)

## 2.4 سياق الكتالوج والمنيو (Catalog & Menu Context)
**المسؤولية:** إدارة المنتجات، الفئات، الإضافات (Modifiers)، والوصفات.
- **Aggregates:** 
  - `Product` (المنتج)
  - `Category` (الفئة)
  - `ModifierGroup` (مجموعة الإضافات)
  - `Menu` (المنيو المخصص للفروع أو التطبيقات)

## 2.5 سياق المخزون والمشتريات (Inventory & Purchasing Context)
**المسؤولية:** إدارة المستودعات، حركات المخزون، الهدر، والموردين.
- **Aggregates:** 
  - `Warehouse` (المستودع)
  - `StockItem` (عنصر المخزون)
  - `StockMovement` (حركة مخزنية: استلام، هدر، تحويل)
  - `Supplier` (المورد)

## 2.6 سياق المحاسبة والمالية (Accounting & Finance Context)
**المسؤولية:** دليل الحسابات، القيود اليومية، الضرائب (ZATCA)، الأصول.
- **Aggregates:** 
  - `Account` (الحساب المالي)
  - `JournalEntry` (القيد اليومي)
  - `TaxProfile` (الملف الضريبي)

## 2.7 سياق الموارد البشرية (HR & Payroll Context)
**المسؤولية:** إدارة الموظفين، الحضور، الرواتب.
- **Aggregates:** 
  - `Employee` (الموظف)
  - `AttendanceRecord` (سجل الحضور)
  - `PayrollPayslip` (مسير الرواتب)

---

# 3. خريطة السياق والاتصال (Context Map & Integration)

بما أننا نستخدم معمارية (Modular Monolith) قابلة للتحول إلى (Microservices):
- لا يُسمح بالاتصال المباشر بقواعد بيانات السياقات الأخرى (No direct DB joins across contexts).
- الاتصال المتزامن (Synchronous): يتم عبر استدعاء واجهات (Interfaces/Contracts) في طبقة الـ Application.
- الاتصال غير المتزامن (Asynchronous): يتم عبر أحداث النطاق والتكامل (Domain & Integration Events).

### أمثلة على التكامل بالأحداث (Event-Driven Integration):
1. عند إغلاق طلب في (POS Context) يطلق حدث `OrderClosedEvent`.
2. يستمع (Inventory Context) للحدث، ويقوم بخصم المكونات بناءً على الوصفة.
3. يستمع (Accounting Context) لنفس الحدث، ويقوم بتوليد `JournalEntry` لتسجيل الإيراد والضريبة.

---

# 4. نمذجة الـ Aggregates الأساسية (Core Domain Models)

## 4.1 نموذج الطلب (Order Aggregate)
- **الجذر (Root):** `Order`
- **الكيانات الداخلية (Entities):** `OrderItem`, `OrderDiscount`, `OrderTax`
- **كائنات القيمة (Value Objects):** `Money` (للمبالغ)، `Address` (للتوصيل)
- **القواعد:** لا يمكن تعديل الطلب بعد إغلاقه.

## 4.2 نموذج الحساب المالي (Account Aggregate)
- **الجذر (Root):** `Account`
- **القواعد:** الحساب الرئيسي (Group) لا يقبل حركات مباشرة. يرث نوع الحساب (أصل، خصم..) من الحساب الأب.

---

نهاية الوثيقة
