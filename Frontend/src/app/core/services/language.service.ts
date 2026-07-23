import { Injectable, signal, effect, computed } from '@angular/core';
import { I18N_TRANSLATIONS } from './i18n-translations';

export type AppLang = 'en' | 'ar';

@Injectable({
  providedIn: 'root'
})
export class LanguageService {
  private readonly LANG_KEY = 'gastro-erp-lang';

  // Signals state
  language = signal<AppLang>('en');

  // Computed state
  direction = computed<string>(() => (this.language() === 'ar' ? 'rtl' : 'ltr'));

  // Simple key-value dictionary for Arabic/English translations
  private translations: Record<string, Record<AppLang, string>> = {
    // Nav Items
    'nav.dashboard': { en: 'Dashboard', ar: 'لوحة القيادة' },
    'nav.organization': { en: 'Organization', ar: 'المنشأة' },
    'nav.inventory': { en: 'Inventory', ar: 'المخزون' },
    'nav.inventory.list': { en: 'Stock List', ar: 'قائمة المخزون' },
    'nav.inventory.items': { en: 'Item Definition', ar: 'تعريف الأصناف' },
    'nav.catalog.engine': { en: 'Product Catalog', ar: 'كatalog المنتجات' },
    'nav.catalog.new': { en: 'New Product', ar: 'منتج جديد' },
    'nav.menu': { en: 'Menus', ar: 'القوائم' },
    'nav.pos': { en: 'POS', ar: 'نقطة البيع' },
    'nav.sales': { en: 'Sales', ar: 'المبيعات' },
    'nav.kitchen': { en: 'Kitchen Management', ar: 'إدارة المطبخ والخدمات' },
    'nav.kitchenDisplay': { en: 'Kitchen Display', ar: 'شاشة الطباخ' },
    'nav.delivery': { en: 'Delivery', ar: 'التوصيل' },
    'nav.crm': { en: 'CRM & Loyalty', ar: 'العملاء والولاء' },
    'nav.finance': { en: 'Finance', ar: 'المالية' },
    'nav.finance.section.operations': { en: 'Financial Operations', ar: 'العمليات المالية' },
    'nav.finance.section.coding': { en: 'Accounting Master Data', ar: 'الترميز المحاسبي' },
    'nav.financialOps': { en: 'Finance', ar: 'المالية' },
    'nav.operations': { en: 'Operations', ar: 'العمليات' },
    'nav.hr': { en: 'Human Resources', ar: 'الموارد البشرية' },
    'nav.reporting': { en: 'Reports', ar: 'التقارير' },
    'nav.marketing': { en: 'Offers & Campaigns', ar: 'العروض والحملات' },
    'nav.settings': { en: 'Settings', ar: 'الإعدادات' },
    'nav.settings.workflow': { en: 'Program Organization', ar: 'تنظيم البرنامج' },
    'nav.settings.branding': { en: 'Branding', ar: 'الهوية البصرية' },
    'nav.settings.system': { en: 'System Settings', ar: 'إعدادات النظام' },
    'nav.settings.permissions': { en: 'Permissions', ar: 'إدارة الصلاحيات' },
    'nav.settings.users': { en: 'Users', ar: 'إدارة المستخدمين' },
    'nav.settings.printing': { en: 'Print Settings', ar: 'إعدادات الطباعة' },
    'nav.settings.ai': { en: 'AI Settings', ar: 'إعدادات الذكاء الاصطناعي' },
    'nav.settings.zatca': { en: 'ZATCA Settings', ar: 'إعدادات ZATCA' },
    'nav.settings.licenses': { en: 'Licenses', ar: 'التراخيص' },
    'nav.settings.devices': { en: 'Devices', ar: 'إدارة الأجهزة' },
    'nav.settings.general': { en: 'General Settings', ar: 'الإعدادات العامة' },
    'nav.branding': { en: 'Visual Identity', ar: 'الهوية البصرية' },
    'nav.media': { en: 'Media Library', ar: 'مكتبة الوسائط' },
    'nav.workflow': { en: 'Workflows', ar: 'سير العمل' },
    'nav.purchases': { en: 'Purchases', ar: 'المشتريات' },

    // Finance
    'finance.title': { en: 'Finance & Accounting', ar: 'المالية والمحاسبة' },
    'finance.subtitle': { en: 'Track double-entry general ledger records, operational expenses, and company net profit metrics.', ar: 'متابعة القيود المحاسبية، المصروفات التشغيلية، ومؤشرات صافي الربح.' },
    'finance.stat.ledger': { en: 'General Ledger Balance', ar: 'رصيد دفتر الأستاذ' },
    'finance.stat.ledgerSub': { en: 'Total cash assets balance', ar: 'إجمالي رصيد النقدية' },
    'finance.stat.profit': { en: 'Net Profit (Current Month)', ar: 'صافي الربح (الشهر الحالي)' },
    'finance.stat.profitSub': { en: 'Subtotal revenues minus costs', ar: 'الإيرادات ناقص التكاليف' },
    'finance.stat.liability': { en: 'Outstanding Liability', ar: 'الالتزامات المستحقة' },
    'finance.stat.liabilitySub': { en: 'Pending supplier payments', ar: 'مدفوعات الموردين المعلقة' },
    'finance.ledgerTitle': { en: 'General Ledger Entries', ar: 'قيود دفتر الأستاذ' },
    'finance.col.code': { en: 'Transaction Code', ar: 'رمز العملية' },
    'finance.col.desc': { en: 'Description', ar: 'الوصف' },
    'finance.col.type': { en: 'Type', ar: 'النوع' },
    'finance.col.amount': { en: 'Amount', ar: 'المبلغ' },
    'finance.col.date': { en: 'Posting Date', ar: 'تاريخ القيد' },
    'finance.type.revenue': { en: 'Revenue', ar: 'إيراد' },
    'finance.type.expense': { en: 'Expense', ar: 'مصروف' },

    // CRM
    'crm.title': { en: 'CRM & Guest Loyalty', ar: 'العملاء وبرنامج الولاء' },
    'crm.subtitle': { en: 'Manage customer relationship profiles, reward tiers, and promotional discount lists.', ar: 'إدارة ملفات العملاء، مستويات المكافآت، والعروض الترويجية.' },
    'crm.stat.guests': { en: 'Registered Guests', ar: 'العملاء المسجلون' },
    'crm.stat.guestsSub': { en: 'Corporate customer count', ar: 'عدد عملاء الشركات' },
    'crm.stat.points': { en: 'Loyalty Points Issued', ar: 'نقاط الولاء المصدرة' },
    'crm.stat.pointsSub': { en: 'Total points balance', ar: 'إجمالي رصيد النقاط' },
    'crm.stat.vip': { en: 'VIP Guests Ratio', ar: 'نسبة عملاء VIP' },
    'crm.stat.vipSub': { en: 'Gold & Platinum members', ar: 'أعضاء الذهبي والبلاتيني' },
    'crm.tableTitle': { en: 'Loyalty Member Profiles', ar: 'ملفات أعضاء الولاء' },
    'crm.col.id': { en: 'Customer ID', ar: 'رقم العميل' },
    'crm.col.name': { en: 'Full Name', ar: 'الاسم الكامل' },
    'crm.col.points': { en: 'Loyalty Points', ar: 'نقاط الولاء' },
    'crm.col.tier': { en: 'Loyalty Tier', ar: 'مستوى الولاء' },
    'crm.col.phone': { en: 'Phone', ar: 'الهاتف' },
    'crm.col.actions': { en: 'Actions', ar: 'إجراءات' },

    // Dashboard
    'dash.title': { en: 'Operations Dashboard', ar: 'لوحة العمليات' },
    'dash.subtitle': { en: 'Customize your administrative widgets layout. Drag, toggle, and pin analytics tiles.', ar: 'خصّص تخطيط لوحة التحكم: إظهار/إخفاء وترتيب عناصر التحليلات.' },
    'dash.customize': { en: 'Customize Tiles', ar: 'تخصيص العناصر' },
    'dash.saveLayout': { en: 'Save Layout', ar: 'حفظ التخطيط' },
    'dash.resetGrid': { en: 'Reset Grid', ar: 'إعادة الضبط' },
    'dash.widget.sales': { en: 'Sales & Revenue Analytics', ar: 'تحليلات المبيعات والإيرادات' },
    'dash.widget.kds': { en: 'Active Kitchen Queues', ar: 'طوابير المطبخ النشطة' },
    'dash.widget.stock': { en: 'Low Stock & Alerts', ar: 'تنبيهات نقص المخزون' },
    'dash.widget.hr': { en: 'Employee Work Profile', ar: 'ملف الموظف الوظيفي' },
    'dash.kds.new': { en: 'New Tickets Queue', ar: 'طلبات جديدة' },
    'dash.kds.preparing': { en: 'Preparing Orders', ar: 'قيد التحضير' },
    'dash.kds.ready': { en: 'Completed Ready', ar: 'جاهز للتسليم' },
    'dash.stock.low': { en: 'Low Stock', ar: 'مخزون منخفض' },
    'dash.stock.out': { en: 'Out of Stock', ar: 'نفد المخزون' },
    'dash.hr.active': { en: 'ACTIVE', ar: 'نشط' },
    'dash.hr.attendance': { en: 'Attendance', ar: 'الحضور' },
    'dash.hr.tenure': { en: 'Tenure', ar: 'مدة الخدمة' },

    'ed.title': { en: 'Executive Dashboard', ar: 'لوحة التحكم التنفيذية' },
    'ed.lastSync': { en: 'Last sync', ar: 'آخر مزامنة' },
    'ed.customize': { en: 'Customize', ar: 'تخصيص' },
    'ed.saveLayout': { en: 'Save layout', ar: 'حفظ التخطيط' },
    'ed.reset': { en: 'Reset', ar: 'إعادة ضبط' },
    'ed.dragHint': { en: 'Drag widgets to reorder while customizing.', ar: 'اسحب العناصر لإعادة ترتيبها أثناء التخصيص.' },
    'ed.loading': { en: 'Loading dashboard…', ar: 'جاري تحميل اللوحة…' },
    'ed.loadFailed': { en: 'Could not load dashboard data.', ar: 'تعذر تحميل بيانات اللوحة.' },
    'ed.apply': { en: 'Apply', ar: 'تطبيق' },
    'ed.empty': { en: 'No data for this period.', ar: 'لا توجد بيانات لهذه الفترة.' },
    'ed.updated': { en: 'Updated', ar: 'آخر تحديث' },
    'ed.min': { en: 'min', ar: 'د' },
    'ed.createPo': { en: 'Create PO', ar: 'إنشاء أمر شراء' },
    'ed.stockOk': { en: 'No low-stock items.', ar: 'لا توجد أصناف منخفضة المخزون.' },
    'ed.noAlerts': { en: 'No notifications.', ar: 'لا توجد تنبيهات.' },
    'ed.period.today': { en: 'Today', ar: 'اليوم' },
    'ed.period.yesterday': { en: 'Yesterday', ar: 'أمس' },
    'ed.period.week': { en: 'This week', ar: 'هذا الأسبوع' },
    'ed.period.month': { en: 'This month', ar: 'هذا الشهر' },
    'ed.period.custom': { en: 'Custom', ar: 'مخصص' },
    'ed.widget.kpis': { en: 'KPI Cards', ar: 'مؤشرات الأداء' },
    'ed.widget.sales': { en: 'Sales Analytics', ar: 'تحليلات المبيعات' },
    'ed.widget.revenueSources': { en: 'Revenue Sources', ar: 'مصادر الإيراد' },
    'ed.widget.payments': { en: 'Payment Methods', ar: 'طرق الدفع' },
    'ed.widget.topItems': { en: 'Top Selling Items', ar: 'الأصناف الأكثر مبيعاً' },
    'ed.widget.worstItems': { en: 'Worst Selling Items', ar: 'الأصناف الراكدة' },
    'ed.widget.topCustomers': { en: 'Top Customers', ar: 'أفضل العملاء' },
    'ed.widget.lowInventory': { en: 'Low Inventory', ar: 'نواقص المخزون' },
    'ed.widget.kitchen': { en: 'Kitchen Status', ar: 'حالة المطبخ' },
    'ed.widget.delivery': { en: 'Delivery', ar: 'التوصيل' },
    'ed.widget.hr': { en: 'Employees', ar: 'الموظفون' },
    'ed.widget.finance': { en: 'Financial Snapshot', ar: 'لمحة مالية' },
    'ed.widget.activities': { en: 'Recent Activities', ar: 'آخر العمليات' },
    'ed.widget.notifications': { en: 'Notifications', ar: 'التنبيهات' },
    'ed.widget.insights': { en: 'AI Insights', ar: 'رؤى ذكية' },
    'ed.widget.quickActions': { en: 'Quick Actions', ar: 'إجراءات سريعة' },
    'ed.kpi.sales': { en: "Today's Sales", ar: 'مبيعات اليوم' },
    'ed.kpi.orders': { en: 'Orders', ar: 'الطلبات' },
    'ed.kpi.avg_ticket': { en: 'Average Ticket', ar: 'متوسط الفاتورة' },
    'ed.kpi.gross_profit': { en: 'Gross Profit', ar: 'إجمالي الربح' },
    'ed.kpi.net_profit': { en: 'Net Profit', ar: 'صافي الربح' },
    'ed.kpi.cash': { en: 'Cash In Drawer', ar: 'النقدية بالصندوق' },
    'ed.kpi.ar': { en: 'Accounts Receivable', ar: 'الذمم المدينة' },
    'ed.kpi.ap': { en: 'Accounts Payable', ar: 'الذمم الدائنة' },
    'ed.kpi.inventory_value': { en: 'Inventory Value', ar: 'قيمة المخزون' },
    'ed.kpi.low_stock': { en: 'Low Stock Items', ar: 'أصناف منخفضة المخزون' },
    'ed.kpi.customers': { en: 'Customers Today', ar: 'عملاء اليوم' },
    'ed.kpi.cancelled': { en: 'Cancelled Orders', ar: 'طلبات ملغاة' },
    'ed.legend.sales': { en: 'Sales', ar: 'المبيعات' },
    'ed.legend.profit': { en: 'Profit', ar: 'الربح' },
    'ed.legend.discounts': { en: 'Discounts', ar: 'الخصومات' },
    'ed.legend.tax': { en: 'Tax', ar: 'الضرائب' },
    'ed.col.item': { en: 'Item', ar: 'الصنف' },
    'ed.col.qty': { en: 'Qty', ar: 'الكمية' },
    'ed.col.revenue': { en: 'Revenue', ar: 'الإيراد' },
    'ed.col.name': { en: 'Name', ar: 'الاسم' },
    'ed.col.orders': { en: 'Orders', ar: 'الطلبات' },
    'ed.col.spend': { en: 'Spend', ar: 'الإنفاق' },
    'ed.col.lastVisit': { en: 'Last visit', ar: 'آخر زيارة' },
    'ed.col.current': { en: 'Current', ar: 'الحالي' },
    'ed.col.min': { en: 'Min', ar: 'الحد الأدنى' },
    'ed.col.suggested': { en: 'Suggested', ar: 'المقترح' },
    'ed.kitchen.pending': { en: 'Pending', ar: 'معلق' },
    'ed.kitchen.preparing': { en: 'Preparing', ar: 'قيد التحضير' },
    'ed.kitchen.ready': { en: 'Ready', ar: 'جاهز' },
    'ed.kitchen.served': { en: 'Served', ar: 'تم التقديم' },
    'ed.kitchen.delayed': { en: 'Delayed', ar: 'متأخر' },
    'ed.kitchen.avg': { en: 'Avg prep', ar: 'متوسط التحضير' },
    'ed.delivery.inProgress': { en: 'In progress', ar: 'جارية' },
    'ed.delivery.delivered': { en: 'Delivered', ar: 'مُسلَّم' },
    'ed.delivery.delayed': { en: 'Delayed', ar: 'متأخر' },
    'ed.delivery.avg': { en: 'Avg delivery', ar: 'متوسط التوصيل' },
    'ed.hr.present': { en: 'Present', ar: 'حضور' },
    'ed.hr.absent': { en: 'Absent', ar: 'غياب' },
    'ed.hr.late': { en: 'Late', ar: 'متأخرون' },
    'ed.hr.hours': { en: 'Worked hours', ar: 'ساعات العمل' },
    'ed.fin.bank': { en: 'Bank', ar: 'البنك' },
    'ed.fin.cash': { en: 'Cash', ar: 'النقدية' },
    'ed.fin.ar': { en: 'Receivables', ar: 'الذمم' },
    'ed.fin.ap': { en: 'Payables', ar: 'الالتزامات' },
    'ed.fin.profit': { en: 'Profit', ar: 'الأرباح' },
    'ed.qa.sale': { en: 'Sales invoice', ar: 'فاتورة بيع' },
    'ed.qa.po': { en: 'Purchase order', ar: 'طلب شراء' },
    'ed.qa.return': { en: 'Return', ar: 'مرتجع' },
    'ed.qa.receipt': { en: 'Receipt voucher', ar: 'سند قبض' },
    'ed.qa.payment': { en: 'Payment voucher', ar: 'سند صرف' },
    'ed.qa.customer': { en: 'Add customer', ar: 'إضافة عميل' },
    'ed.qa.supplier': { en: 'Add supplier', ar: 'إضافة مورد' },
    'ed.qa.item': { en: 'Add item', ar: 'إضافة صنف' },

    // Menu operations
    'menu.title': { en: 'Menu & Pricing', ar: 'القائمة والتسعير' },
    'menu.subtitle': { en: 'Add products, manage categories, and configure price levels for dine-in, takeaway, and delivery.', ar: 'إضافة المنتجات، إدارة التصنيفات، وضبط مستويات التسعير للمحلي والسفري والتوصيل.' },
    'menu.tab.products': { en: 'Products', ar: 'المنتجات' },
    'menu.tab.pricing': { en: 'Price Levels', ar: 'مستويات التسعير' },
    'menu.tab.categories': { en: 'Categories', ar: 'التصنيفات' },
    'menu.addProduct': { en: 'Add Product', ar: 'إضافة منتج' },
    'menu.editProduct': { en: 'Edit Product', ar: 'تعديل منتج' },
    'menu.productsList': { en: 'Product Catalog', ar: 'كتالوج المنتجات' },
    'menu.priceLevels': { en: 'Price Levels', ar: 'مستويات التسعير' },
    'menu.categories': { en: 'Menu Categories', ar: 'تصنيفات القائمة' },
    'menu.addPriceLevel': { en: 'Add Price Level', ar: 'إضافة مستوى تسعير' },
    'menu.addCategory': { en: 'Add Category', ar: 'إضافة تصنيف' },
    'menu.save': { en: 'Save', ar: 'حفظ' },
    'menu.cancel': { en: 'Cancel', ar: 'إلغاء' },
    'menu.edit': { en: 'Edit', ar: 'تعديل' },
    'menu.delete': { en: 'Delete', ar: 'حذف' },
    'menu.col.sku': { en: 'SKU', ar: 'رمز المنتج' },
    'menu.col.nameAr': { en: 'Name (AR)', ar: 'الاسم بالعربية' },
    'menu.col.nameEn': { en: 'Name (EN)', ar: 'الاسم بالإنجليزية' },
    'menu.col.category': { en: 'Category', ar: 'التصنيف' },
    'menu.col.dineIn': { en: 'Dine In', ar: 'محلي' },
    'menu.col.takeaway': { en: 'Takeaway', ar: 'سفري' },
    'menu.col.delivery': { en: 'Delivery', ar: 'توصيل' },
    'menu.col.actions': { en: 'Actions', ar: 'إجراءات' },
    'menu.field.nameAr': { en: 'Arabic name', ar: 'الاسم بالعربية' },
    'menu.field.nameEn': { en: 'English name', ar: 'الاسم بالإنجليزية' },
    'menu.field.category': { en: 'Category', ar: 'التصنيف' },
    'menu.channel.dineIn': { en: 'Dine In', ar: 'محلي' },
    'menu.channel.takeaway': { en: 'Takeaway', ar: 'سفري' },
    'menu.channel.delivery': { en: 'Delivery', ar: 'توصيل' },

    // HR operations
    'hr.title': { en: 'HR Operations', ar: 'عمليات الموارد البشرية' },
    'hr.subtitle': { en: 'Add employees, manage departments, and view employee profiles.', ar: 'إضافة الموظفين، إدارة الأقسام، وعرض ملفات الموظفين.' },
    'hr.tab.list': { en: 'Employee List', ar: 'قائمة الموظفين' },
    'hr.tab.profile': { en: 'Employee Profile', ar: 'ملف الموظف' },
    'hr.addEmployee': { en: 'Add Employee', ar: 'إضافة موظف' },
    'hr.employeesList': { en: 'Employees', ar: 'الموظفون' },
    'hr.viewProfile': { en: 'View Profile', ar: 'عرض الملف' },
    'hr.profileHint': { en: 'Personal details, contact info, and employment records appear here.', ar: 'تظهر هنا البيانات الشخصية ومعلومات الاتصال وسجل التوظيف.' },
    'hr.col.name': { en: 'Full Name', ar: 'الاسم الكامل' },
    'hr.col.title': { en: 'Job Title', ar: 'المسمى الوظيفي' },
    'hr.col.department': { en: 'Department', ar: 'القسم' },
    'hr.col.email': { en: 'Email', ar: 'البريد الإلكتروني' },
    'hr.field.name': { en: 'Full name', ar: 'الاسم الكامل' },
    'hr.field.title': { en: 'Job title', ar: 'المسمى الوظيفي' },
    'hr.field.department': { en: 'Department', ar: 'القسم' },
    'hr.field.phone': { en: 'Phone', ar: 'الهاتف' },

    // Toolbar
    'toolbar.search': { en: 'Search...', ar: 'بحث...' },
    'toolbar.bookmarks': { en: 'Starred Bookmarks', ar: 'الصفحات المفضلة' },
    'toolbar.noBookmarks': { en: 'No starred pages.', ar: 'لا توجد صفحات مفضلة.' },
    'toolbar.notifications': { en: 'Notifications Feed', ar: 'الإشعارات' },
    'toolbar.noNotifications': { en: 'No notifications.', ar: 'لا توجد إشعارات.' },
    'toolbar.myProfile': { en: 'My Profile', ar: 'ملفي الشخصي' },
    'toolbar.logout': { en: 'Logout', ar: 'تسجيل الخروج' },
    'toolbar.company': { en: 'Company', ar: 'المنشأة' },
    'toolbar.searchPlaceholder': { en: 'Search... Invoice, Customer, Supplier, Item, Account', ar: 'ابحث عن... فاتورة، عميل، مورد، صنف، حساب' },
    'toolbar.quickAdd': { en: 'Quick Add', ar: 'إضافة سريعة' },
    'toolbar.aiState': { en: 'AI Assistant', ar: 'مساعد الذكاء الاصطناعي' },
    'toolbar.aiState.connected': { en: 'Connected', ar: 'متصل' },
    'toolbar.aiState.connecting': { en: 'Connecting', ar: 'جاري الاتصال' },
    'toolbar.aiState.offline': { en: 'Unavailable', ar: 'غير متاح' },
    'toolbar.favorites': { en: 'Favorites', ar: 'المفضلة' },
    'toolbar.license': { en: 'License valid (3644 days remaining)', ar: 'الترخيص صالح (3644 يوم متبقي)' },
    'toolbar.zatca': { en: 'ZATCA Ready', ar: 'ZATCA جاهز' },
    'toolbar.backup': { en: 'Last backup 11:30', ar: 'آخر Backup 11:30' },
    'toolbar.pendingApprovals': { en: '3 pending approvals', ar: '3 عمليات تنتظر الاعتماد' },
    'toolbar.database': { en: 'Database: Connected (MainDB)', ar: 'قاعدة البيانات: متصلة (MainDB)' },

    // HR extended
    'emp.departments': { en: 'Departments', ar: 'الأقسام' },
    'emp.teamMembers': { en: 'Team Members', ar: 'أعضاء الفريق' },
    'emp.tenure': { en: 'Tenure', ar: 'مدة الخدمة' },
    'emp.dept.fnb': { en: 'Food & Beverage', ar: 'الأغذية والمشروبات' },
    'emp.dept.kitchen': { en: 'Kitchen Operations', ar: 'عمليات المطبخ' },
    'emp.dept.logistics': { en: 'Logistics', ar: 'اللوجستيات' },

    // Media
    'media.breadcrumb.portal': { en: 'Portal Workspace', ar: 'مساحة العمل' },
    'media.breadcrumb.library': { en: 'Media Library', ar: 'مكتبة الوسائط' },
    'media.breadcrumb.manage': { en: 'Manage Assets', ar: 'إدارة الأصول' },
    'media.title': { en: 'Enterprise Media Manager', ar: 'مدير الوسائط' },
    'media.subtitle': { en: 'Manage asset folders, perform bulk conversions to WebP, drag & drop files, and crop brand images.', ar: 'إدارة مجلدات الأصول، التحويل الجماعي إلى WebP، السحب والإفلات، وقص صور العلامة التجارية.' },

    // Landing Page
    'landing.title': { en: 'The Future of Restaurant', ar: 'مستقبل إدارة' },
    'landing.subtitle': { en: 'Enterprise Management', ar: 'منشآت المطاعم الكبرى' },
    'landing.description': { 
      en: 'A high-performance management ecosystem for premium hospitality groups. Scale your operations with AI-driven insights and cinematic efficiency.', 
      ar: 'نظام إدارة عالي الأداء لمجموعات الضيافة الفاخرة. ارتقِ بعملياتك التشغيلية برؤى مدعومة بالذكاء الاصطناعي وكفاءة متناهية.' 
    },
    'landing.trial': { en: 'Start Free Trial', ar: 'ابدأ التجربة المجانية' },
    'landing.demo': { en: 'Request Demo', ar: 'طلب عرض توضيحي' },

    // Login Screen
    'login.title': { en: 'Sign in to GastroERP', ar: 'تسجيل الدخول إلى GastroERP' },
    'login.subtitle': { en: 'Enter your credentials to manage your enterprise', ar: 'أدخل بيانات الاعتماد الخاصة بك لإدارة منشأتك' },
    'login.welcomeBack': { en: 'Welcome back', ar: 'مرحباً بعودتك' },
    'login.welcomeSubtitle': { en: 'Access your management dashboard.', ar: 'الوصول إلى لوحة التحكم الإدارية الخاصة بك.' },
    'login.promoText': {
      en: 'Unify your operations with the industry\'s most powerful restaurant management ecosystem. Scalable, secure, and designed for high-performance hospitality teams.',
      ar: 'وحد عملياتك التشغيلية مع أقوى نظام إدارة مطاعم في القطاع. مرن، آمن، ومصمم لفرق الضيافة ذات الأداء العالي.'
    },
    'login.trustedBy': { en: 'Trusted by 500+ Luxury Brands', ar: 'موثوق به من قبل أكثر من ٥٠٠ علامة تجارية فاخرة' },
    'login.email': { en: 'Email Address', ar: 'البريد الإلكتروني' },
    'login.password': { en: 'Password', ar: 'كلمة المرور' },
    'login.rememberMe': { en: 'Remember this device for 30 days', ar: 'تذكر هذا الجهاز لمدة ٣٠ يوماً' },
    'login.signInBtn': { en: 'Sign In', ar: 'تسجيل الدخول' },
    'login.signingIn': { en: 'Signing in…', ar: 'جارٍ تسجيل الدخول…' },
    'login.forgotPassword': { en: 'Forgot password?', ar: 'نسيت كلمة المرور؟' },
    'login.contactAdmin': { en: 'Don\'t have an account? Contact Administrator', ar: 'ليس لديك حساب؟ اتصل بمسؤول النظام' },
    'login.privacy': { en: 'Privacy Policy', ar: 'سياسة الخصوصية' },
    'login.terms': { en: 'Terms of Service', ar: 'شروط الخدمة' },
    'login.help': { en: 'Help Center', ar: 'مركز المساعدة' },

    // Setup Wizard
    'wizard.title': { en: 'Enterprise Suite Setup Wizard', ar: 'معالج إعداد المنشأة والمجموعات' },
    'wizard.step.identity': { en: '01 Admin', ar: '٠١ المسؤول' },
    'wizard.step.venue': { en: '02 Venue', ar: '٠٢ المنشأة' },
    'wizard.step.location': { en: '03 Location', ar: '٠٣ الموقع' },
    'wizard.step.review': { en: '04 Review', ar: '٠٤ المراجعة' },
    'wizard.admin.title': { en: 'Step 1: Primary Administrator', ar: 'الخطوة ١: مسؤول النظام الرئيسي' },
    'wizard.admin.subtitle': { en: 'Configure the master credentials for your enterprise headquarters.', ar: 'قم بتهيئة بيانات الاعتماد الرئيسية لمقر إدارة منشأتك.' },
    'wizard.admin.fullName': { en: 'Full Name', ar: 'الاسم الكامل' },
    'wizard.admin.mobile': { en: 'Mobile Number', ar: 'رقم الهاتف' },
    'wizard.admin.confirmPassword': { en: 'Confirm Password', ar: 'تأكيد كلمة المرور' },
    'wizard.venue.title': { en: 'Step 2: Company Information', ar: 'الخطوة ٢: معلومات الشركة والمنشأة' },
    'wizard.venue.subtitle': { en: 'Tell us about your culinary enterprise to personalize your workspace.', ar: 'أخبرنا عن نشاطك التجاري في مجال المأكولات لتخصيص مساحة العمل.' },
    'wizard.venue.legalName': { en: 'Legal Entity Name', ar: 'اسم الكيان القانوني' },
    'wizard.venue.industry': { en: 'Industry Type', ar: 'نوع النشاط التشغيلي' },
    'wizard.venue.branches': { en: 'Initial Branch Count', ar: 'عدد الفروع المبدئي' },
    'wizard.venue.address': { en: 'Headquarters Address', ar: 'عنوان المقر الرئيسي' },
    'wizard.location.title': { en: 'Step 3: Branch Location', ar: 'الخطوة ٣: موقع الفرع الرئيسي' },
    'wizard.location.subtitle': { en: 'Provide the physical address for this branch. This is information for delivery zones, tax calculations, and localization settings.', ar: 'أدخل العنوان الفعلي لهذا الفرع لتحديد مناطق التوصيل، حساب الضرائب، وإعدادات الموقع.' },
    'wizard.location.country': { en: 'Country', ar: 'الدولة' },
    'wizard.location.city': { en: 'City', ar: 'المدينة' },
    'wizard.location.state': { en: 'State / Province / District', ar: 'الولاية / المنطقة / المحافظة' },
    'wizard.location.street': { en: 'Street Name', ar: 'اسم الشارع' },
    'wizard.location.bldg': { en: 'Bldg / Apt', ar: 'المبنى / رقم الشقة' },
    'wizard.location.zip': { en: 'Postal / Zip Code', ar: 'الرمز البريدي' },
    'wizard.location.detect': { en: 'Detect location', ar: 'تحديد الموقع تلقائياً' },
    'wizard.review.title': { en: 'Final Review', ar: 'المراجعة النهائية وتأكيد الإعدادات' },
    'wizard.review.subtitle': { en: 'Please review the details below before writing your enterprise database.', ar: 'يرجى مراجعة التفاصيل أدناه قبل إنشاء وتخزين قاعدة بيانات منشأتك.' },
    'wizard.btn.back': { en: 'Back', ar: 'السابق' },
    'wizard.btn.proceed': { en: 'Proceed to Venue Info', ar: 'الاستمرار لبيانات المنشأة' },
    'wizard.btn.proceed2': { en: 'Continue to Location', ar: 'الاستمرار لبيانات الموقع' },
    'wizard.btn.proceed3': { en: 'Review Settings', ar: 'مراجعة وتأكيد البيانات' },
    'wizard.btn.create': { en: 'Create Restaurant', ar: 'بدء تهيئة النظام وتأسيس المطعم' },

    // App shell
    'shell.branch': { en: 'Branch: Downtown', ar: 'الفرع: وسط المدينة' },
    'shell.search': { en: 'Search menu items, files, employees...', ar: 'بحث في قائمة الطعام، الملفات، الموظفين...' },

    // POS
    'pos.order': { en: 'Current Order', ar: 'الطلب الحالي' },
    'pos.split': { en: 'SPLIT BILL', ar: 'تقسيم الفاتورة' },
    'pos.kitchen': { en: 'KITCHEN', ar: 'إرسال للمطبخ' },
    'pos.pay': { en: 'Pay Now', ar: 'ادفع الآن' },
    'pos.notes': { en: 'Add notes here...', ar: 'إضافة ملاحظات هنا...' },

    // KDS
    'kds.bump': { en: 'BUMP', ar: 'إنجاز' },
    'kds.new': { en: 'NEW', ar: 'جديد' },
    'kds.preparing': { en: 'PREPARING', ar: 'قيد التحضير' },
    'kds.ready': { en: 'READY', ar: 'جاهز للتسليم' },

    // Inventory
    'inventory.totalValue': { en: 'Total Value', ar: 'القيمة الإجمالية للمخزون' },
    'inventory.lowStockAlerts': { en: 'Low Stock Alerts', ar: 'تنبيهات نقص المخزون' },
    'inventory.expiringBatches': { en: 'Expiring Batches', ar: 'شحنات تقترب صلاحيتها' },
    'inventory.masterList': { en: 'Inventory Master List', ar: 'القائمة الرئيسية للمخزون' },
    'inventory.search': { en: 'Search inventory items...', ar: 'بحث في بنود المخزون...' },
    'inventory.export': { en: 'Export CSV', ar: 'تصدير CSV' },
    'inventory.col.item': { en: 'ITEM NAME', ar: 'اسم البند' },
    'inventory.col.batch': { en: 'BATCH NUMBER', ar: 'رقم الشحنة' },
    'inventory.col.location': { en: 'LOCATION', ar: 'موقع التخزين' },
    'inventory.col.stock': { en: 'STOCK LEVEL', ar: 'كمية المخزون' },
    'inventory.col.price': { en: 'UNIT PRICE', ar: 'سعر الوحدة' },
    'inventory.col.value': { en: 'TOTAL VALUE', ar: 'القيمة الإجمالية' },
    'inventory.col.status': { en: 'STATUS', ar: 'حالة المخزون' },

    // Employees
    'emp.title': { en: 'Julian Sterling', ar: 'جوليان ستيرلينغ' },
    'emp.role': { en: 'Executive Chef', ar: 'رئيس الطهاة التنفيذي' },
    'emp.location': { en: 'Downtown Branch', ar: 'فرع وسط المدينة' },
    'emp.personalInfo': { en: 'Personal Info', ar: 'المعلومات الشخصية' },
    'emp.attendance': { en: 'Attendance', ar: 'سجل الحضور' },
    'emp.leaves': { en: 'Leaves', ar: 'الإجازات' },
    'emp.contracts': { en: 'Contracts', ar: 'العقود والرواتب' },
    'emp.performance': { en: 'Performance', ar: 'الأداء الوظيفي' },

    ...I18N_TRANSLATIONS
  };

  constructor() {
    const savedLang = localStorage.getItem(this.LANG_KEY) as AppLang;
    if (savedLang) {
      this.language.set(savedLang);
    } else {
      this.language.set('ar');
    }

    // Effect to apply dir on document tag
    effect(() => {
      const currentLang = this.language();
      localStorage.setItem(this.LANG_KEY, currentLang);
      
      const dir = currentLang === 'ar' ? 'rtl' : 'ltr';
      document.documentElement.setAttribute('dir', dir);
      document.documentElement.setAttribute('lang', currentLang);
    });
  }

  toggleLanguage(): void {
    this.language.update(current => current === 'en' ? 'ar' : 'en');
  }

  setLanguage(lang: AppLang): void {
    this.language.set(lang);
  }

  // Translation helper
  t(key: string): string {
    const term = this.translations[key];
    if (!term) return key;
    return term[this.language()] || key;
  }

  tFor(key: string, lang: AppLang): string {
    const term = this.translations[key];
    if (!term) return key;
    return term[lang] || key;
  }
}
