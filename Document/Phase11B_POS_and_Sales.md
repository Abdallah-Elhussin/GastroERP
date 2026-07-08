# المرحلة 11B: خارطة طريق وحدة نقاط البيع والمبيعات (POS and Sales)

## 1. نظرة عامة
تُعد وحدة **نقاط البيع والمبيعات (POS and Sales)** القلب النابض لنظام GastroERP، حيث تتولى معالجة معاملات العملاء، وإدارة الطلبات، وتحصيل المدفوعات. تركز هذه المرحلة على توسيع بنية (DDD/CQRS) المُؤسسة مسبقاً لدعم عمليات نقاط البيع عالية الأداء وذات الموثوقية العالية.

## 2. كيانات النطاق الأساسية (Core Domain Entities)

### 2.1 الطلب وعناصر الطلب (Order & OrderItem)
- **الطلب (Order)**: يُمثل معاملة العميل (محلي/Dine-in، سفري/Takeaway، توصيل/Delivery).
  - الخصائص: `OrderNumber`، `OrderType` (محلي، سفري، توصيل)، `Status` (قيد الانتظار، قيد التحضير، جاهز، مُقدم، مُلغى، مكتمل)، `TotalAmount`، `DiscountAmount`، `TaxAmount`، `FinalAmount`، `Notes`.
  - العلاقات: `CustomerId` (اختياري)، `TableId` (اختياري للطلبات المحلية)، `BranchId`، `TenantId`.
- **عنصر الطلب (OrderItem)**: الأصناف داخل الطلب.
  - الخصائص: `ProductId`، `Quantity`، `UnitPrice`، `TotalPrice`، `Notes`.
  - العلاقات: `OrderId`، `Modifiers` (قائمة الإضافات/التعديلات المطبقة).

### 2.2 الدفع (Payment)
- **الدفعة (Payment)**: تسجل كيفية تسوية الطلب.
  - الخصائص: `PaymentMethod` (نقدي، بطاقة، حوالة بنكية، نقاط)، `Amount`، `ReferenceNumber`، `Status` (معلق، مكتمل، فاشل).
  - العلاقات: `OrderId`، `TenantId`.

### 2.3 الوردية وصندوق الكاشير (Shift & CashDrawer)
- **الوردية (Shift)**: تتبع فترة عمل الكاشير.
  - الخصائص: `StartTime`، `EndTime`، `StartingCash`، `ExpectedCash`، `ActualCash`، `Difference`، `Status` (مفتوح، مغلق).
  - العلاقات: `UserId`، `BranchId`، `TenantId`.
- **حركة صندوق الكاشير (CashDrawerTransaction)**: حركات النقد الداخلة/الخارجة خلال الوردية.
  - الخصائص: `Type` (إيداع/PayIn، سحب/PayOut)، `Amount`، `Reason`.
  - العلاقات: `ShiftId`.

### 2.4 الفاتورة (Invoice)
- **الفاتورة (Invoice)**: إيصال ضريبي للطلب المكتمل.
  - الخصائص: `InvoiceNumber`، `IssueDate`، `SubTotal`، `TaxTotal`، `GrandTotal`، `ZatcaQRCode` (إذا كان مطبقاً لمتطلبات الزكاة والدخل)، `Status`.
  - العلاقات: `OrderId`، `TenantId`.

## 3. خطة تنفيذ أوامر واستعلامات CQRS

### 3.1 الأوامر (Commands)
- **الطلبات**: `CreateOrderCommand`، `UpdateOrderStatusCommand`، `AddOrderItemCommand`، `RemoveOrderItemCommand`، `ApplyDiscountCommand`.
- **المدفوعات**: `ProcessPaymentCommand`، `RefundPaymentCommand`.
- **الورديات**: `OpenShiftCommand`، `CloseShiftCommand`، `AddCashDrawerTransactionCommand`.
- **الفواتير**: `GenerateInvoiceCommand`.

### 3.2 الاستعلامات (Queries)
- **الطلبات**: `GetOrderByIdQuery`، `GetActiveOrdersQuery`، `GetOrderHistoryQuery`.
- **المدفوعات**: `GetPaymentsByOrderQuery`، `GetDailyRevenueQuery`.
- **الورديات**: `GetShiftDetailsQuery`، `GetActiveShiftQuery`.

## 4. مسارات واجهة برمجة التطبيقات (API Controllers)
- مُتحكم الطلبات `OrdersController`: `/api/orders`
- مُتحكم المدفوعات `PaymentsController`: `/api/payments`
- مُتحكم الورديات `ShiftsController`: `/api/shifts`
- مُتحكم الفواتير `InvoicesController`: `/api/invoices`

## 5. الأمان وتعدد المستأجرين (Security & Multi-Tenancy)
- يجب أن تطبق جميع الكيانات واجهة `IMustHaveTenant` أو `IMayHaveTenant`.
- يجب تصفية وفلترة جميع الطلبات بناءً على `TenantId`.
- يجب أن تتحقق سياسات الصلاحيات (Authorization) من صلاحيات نقاط البيع (مثل: `Permissions.POS.CreateOrder`، `Permissions.POS.ProcessPayment`، `Permissions.POS.CloseShift`).

## 6. خطوات التنفيذ (العمل القادم)
1. **طبقة النطاق (Domain Layer)**: تعريف الكيانات، التعدادات (Enums)، كائنات القيم (Value Objects)، وأحداث النطاق (Domain Events).
2. **طبقة التطبيق (Application Layer)**: إنشاء الـ DTOs، أوامر واستعلامات CQRS، أدوات التحقق (Validators)، وخرائط AutoMapper.
3. **طبقة البنية التحتية (Infrastructure Layer)**: (اختياري) التكامل مع بوابات الدفع الخارجية أو هيئة الزكاة (ZATCA) للفوترة الإلكترونية.
4. **طبقة قواعد البيانات (Persistence Layer)**: إنشاء إعدادات EF Core بدقة رقمية صارمة (18,4) وقيود تفرد (مثل رقم الطلب لكل مستأجر).
5. **طبقة العرض (Presentation Layer)**: فتح واجهات REST API مع توثيق Swagger.
6. **الاختبار (Testing)**: كتابة اختبارات الوحدات لقواعد الأعمال (مثال: عدم السماح بإغلاق وردية فارغة، عدم السماح بعملية دفع تتجاوز إجمالي الطلب).

---
**تم الإعداد بواسطة**: مهندس البرمجيات الرئيسي (Lead Software Architect)
**الحالة**: جاهز للتنفيذ
