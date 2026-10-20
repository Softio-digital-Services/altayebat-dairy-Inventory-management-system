/* Al-Tayebat Web UI - SQLite backend via /api/ */
const API = '';
const T = {
    en: {
        login_title: 'Sign In', login_subtitle: 'Inventory management portal',
        username: 'Username', password: 'Password', login_btn: 'Sign In', logout: 'Logout',
        nav_dashboard: 'Dashboard', nav_inventory: 'Inventory', nav_pos: 'POS', nav_sales: 'Sales',
        nav_reports: 'Reports', nav_history: 'History', nav_quotations: 'Quotations',
        nav_customers: 'Customers', nav_suppliers: 'Suppliers', nav_expenses: 'Expenses',
        nav_currencies: 'Currencies', nav_barcodes: 'Barcodes', nav_users: 'Users', nav_settings: 'Settings',
        dash_subtitle: 'Overview of your inventory business', inv_subtitle: 'Manage parts and stock levels',
        pos_subtitle: 'Scan or tap products to sell', sales_subtitle: 'Order history and returns',
        reports_subtitle: 'Sales and profit summaries', history_subtitle: 'Audit trail and activity',
        quot_subtitle: 'Quotes waiting to convert', cust_subtitle: 'Customer directory',
        supp_subtitle: 'Supplier directory', exp_subtitle: 'Track business expenses',
        cur_subtitle: 'Exchange rates vs USD', bar_subtitle: 'Preview labels for printing',
        barcode_preview_title: 'Print',
        select_all: 'Select all',
        select_barcodes_first: 'Select at least one barcode to print',
        print_destination: 'Destination',
        print_copies: 'Copies',
        print_layout: 'Layout',
        print_portrait: 'Portrait',
        print_landscape: 'Landscape',
        print_pages: 'Pages',
        print_custom: 'Custom',
        print_pages_ph: 'e.g. 1-5, 8',
        print_color: 'Color',
        print_color_full: 'Color',
        print_color_bw: 'Black and white',
        print_sheet_one: 'Total: 1 sheet of paper',
        print_sheet_many: 'Total: {0} sheets of paper',
        print_system_printer: 'System printer…',
        print_sent: 'Sent to printer',
        print_failed: 'Print failed',
        print_settings_title: 'Printer & paper size',
        print_settings_hint: 'Set paper size in mm for any printer. Each printer remembers its own size.',
        print_label_size: 'Barcode labels',
        print_receipt_size: 'POS receipt',
        print_preset: 'Preset',
        print_width_mm: 'Width (mm)',
        print_height_mm: 'Height (mm)',
        print_label_w: 'Label W (mm)',
        print_label_h: 'Label H (mm)',
        print_page_w: 'Page W (mm)',
        print_page_h: 'Page H (mm)',
        print_gap_mm: 'Gap (mm)',
        print_margin_mm: 'Margin (mm)',
        print_margin_top: 'Top (mm)',
        print_margin_right: 'Right (mm)',
        print_margin_bottom: 'Bottom (mm)',
        print_margin_left: 'Left (mm)',
        print_columns: 'Columns',
        print_columns_auto: 'Auto',
        print_default_label_printer: 'Default label printer',
        print_default_receipt_printer: 'Default receipt printer',
        print_profiles_title: 'Saved printer profiles',
        print_profiles_hint: 'Sizes remembered per printer. Remove a profile to reset that device.',
        print_profiles_clear: 'Clear all profiles',
        print_profiles_empty: 'No saved printer profiles yet.',
        print_profile_remove: 'Remove',
        print_paper_mode: 'Paper mode',
        print_mode_sheet: 'Sheet (multi labels)',
        print_mode_roll: 'Label printer (1 per page)',
        print_settings_save: 'Save print settings',
        print_settings_saved: 'Print settings saved',
        print_settings_fail: 'Could not save print settings',
        users_subtitle: 'Accounts and roles', settings_subtitle: 'Language, license and appearance',
        settings_general: 'General', settings_language_hint: 'Choose the app display language.',
        recent_sales: 'Recent Sales', add_product: 'Add Product', add_expense: 'Add Expense',
        search_ph: 'Search...', cart_title: 'Cart', clear: 'Clear', subtotal: 'Subtotal', total: 'Total',
        checkout: 'Checkout', cancel: 'Cancel', save: 'Save', print: 'Print', convert: 'Confirm',
        process_return: 'Process Return', return_reason: 'Reason', return_reason_ph: 'e.g. Defective',
        confirm_return: 'Confirm Return', language: 'Language', license_info: 'License',
        top_products: 'Top Products', best_selling: 'Best Selling Products', top_categories: 'Top Categories',
        mark_paid: 'Mark paid', paid: 'Paid', unpaid: 'Unpaid',
        preset_daily: 'Daily', preset_weekly: 'Weekly', preset_monthly: 'Monthly', preset_yearly: 'Yearly', preset_custom: 'Custom',
        date_from: 'From', date_to: 'To',
        col_order: 'Order', col_date: 'Date', col_customer: 'Customer', col_total: 'Total',
        col_name: 'Name', col_sku: 'SKU', col_category: 'Category', col_price: 'Price',
        col_stock: 'Stock', col_status: 'Status', col_actions: 'Actions', col_phone: 'Phone',
        col_email: 'Email', col_balance: 'Amount owed', col_payment: 'Payment', col_type: 'Type', col_contact: 'Contact',
        col_role: 'Role', col_barcode: 'Barcode', col_qty: 'Qty', col_sales: 'Sales',
        role_admin: 'Admin', role_staff: 'Staff', role_accountant: 'Accountant',
        col_profit: 'Profit', col_amount: 'Amount', col_desc: 'Description', col_code: 'Code',
        col_symbol: 'Symbol', col_rate: 'Rate',
        col_items: 'Items', col_order_id: 'Order ID',
        all: 'All', in_stock: 'In Stock', low_stock: 'Low Stock', out_of_stock: 'Out of Stock',
        pos_out_of_stock_title: 'Out of stock',
        pos_out_of_stock_msg: '"{0}" is out of stock.',
        pos_insufficient_stock_title: 'Insufficient stock',
        pos_insufficient_weight_msg: 'Not enough stock for "{0}".\n\nRequested weight: {1} kg\nAvailable in inventory: {2} kg',
        pos_sell_anyway: 'Sell anyway (no stock)',
        quick_sale: 'Quick Sale',
        quick_sale_hint: 'Sell without adding stock, or ring up a one-off item.',
        quick_sale_existing: 'Existing product',
        quick_sale_custom: 'Custom item',
        quick_sale_skip_stock: "Don't deduct from inventory",
        quick_sale_badge: 'No stock',
        add_to_cart: 'Add to cart',
        qs_pick_product: 'Select a product',
        qs_need_name: 'Enter an item name',
        qs_need_price: 'Enter a valid price',
        ok: 'OK',
        return: 'Return', empty_cart: 'Cart is empty', empty_list: 'No records', loading: 'Loading…',
        login_failed: 'Invalid username or password', checkout_ok: 'Sale completed',
        checkout_fail: 'Checkout failed', product_ok: 'Product added', return_ok: 'Return processed',
        expense_ok: 'Expense saved', convert_ok: 'Confirmed — converted to order',
        today_sales: 'Today Sales', inventory_value: 'Inventory Value', total_items: 'Total Items',
        low_stock_count: 'Low Stock', orders_today: 'Orders Today',
        rep_sales: 'Sales', rep_cost: 'Cost', rep_expenses: 'Expenses', rep_profit: 'Profit',
        rep_profit_before_expenses: 'Profit (before expenses)', rep_profit_after_expenses: 'Profit (after expenses)',
        lic_type: 'Type', lic_customer: 'Customer', lic_expires: 'Expires', lic_days: 'Days left',
        lic_valid: 'Valid', lic_invalid: 'Invalid', lic_trial: 'Trial', lic_machine: 'Machine',
        export: 'Export', tool_notif: 'Notifications', tool_lock: 'Lock', tool_calc: 'Calculator',
        refresh: 'Refresh', add_category: 'Add Category', add_customer: 'Add Customer',
        add_supplier: 'Add Supplier', add_user: 'Add User', add_currency: 'Add Currency',
        edit: 'Edit', delete: 'Delete', confirm_delete: 'Delete this record?', confirm_title: 'Please confirm', confirm_btn: 'Confirm', import: 'Import',
        refresh_rates: 'Refresh Rates', view: 'View', deleted_ok: 'Deleted', saved_ok: 'Saved', import_ok: 'Imported',
        view_order: 'View Order', col_address: 'Address', edit_product: 'Edit Product',
        quote_preview_title: 'Quotation Preview', quote_preview_quote: 'QUOTATION',
        quote_preview_cust_header: 'Customer Details', quote_col_photo: 'Photo', quote_col_desc: 'Item Description', quote_col_qty: 'Qty',
        quote_preview_meta: 'QUOTE #: {0}   |   DATE: {1}   |   CUST ID: {2}   |   VALIDITY: {3}',
        quote_validity_days: '15 Days', quote_no_address: 'No Address Provided', quote_no_phone: 'No Phone Provided',
        quote_terms_head: 'TERMS AND CONDITIONS',
        quote_terms_body: '• Validity: 15 days from issue.\n• Payment due prior to delivery.\n• Acceptance indicates billing confirmation.\n\nAccepted By: __________________________',
        quote_tax_extras: 'Tax / Extras', grand_total: 'GRAND TOTAL', preview: 'Preview',
        tool_backup: 'Backup', tool_about: 'About', backup_now: 'Create Backup', factory_reset: 'Factory Delete', confirm_factory_1: 'Delete ALL data and reset the database? This cannot be undone.', confirm_factory_2: 'Final confirmation: wipe everything and start empty?', factory_ok: 'Database reset', factory_fail: 'Factory reset failed', backup_export_ok: 'Backup exported', backup_import_ok: 'Backup imported',
        open_backup_folder: 'Open Folder', backup_ok: 'Backup created', backup_fail: 'Backup failed',
        auto_backup: 'Auto backup', auto_backup_off: 'Off', auto_backup_daily: 'Daily',
        auto_backup_weekly: 'Weekly', auto_backup_monthly: 'Monthly',
        auto_backup_hint_off: 'Automatic backups are disabled.',
        auto_backup_hint_on: 'Automatic backups run while the app is open.',
        backup_location: 'Backup location', backup_choose_folder: 'Choose Folder', backup_use_default: 'Use Default',
        no_notifications: 'No notifications', clear_all_notifications: 'Clear all',
        clear_notification: 'Clear', confirm_clear_notifications: 'Clear all notifications?',
        app_refreshed: 'App refreshed',
        locked_title: 'Screen Locked',
        locked_subtitle: 'Enter your password to continue', unlock_btn: 'Unlock',
        unlock_fail: 'Incorrect password', about_blurb: 'Desktop inventory management for Al-Tayebat.',
        last_backup: 'Last backup', backup_none: 'No backup yet',
        activate_license: 'Activate License', activate_btn: 'Activate', start_trial: 'Start Trial',
        license_key: 'License Key', machine_id: 'Machine ID', copy: 'Copy',
        license_subtitle: 'Enter your license key to activate this machine.',
        license_ok: 'License activated successfully', license_invalid: 'Invalid license key',
        trial_expired_msg: 'Your trial period has ended. Please enter a valid license key to activate the app.',
        license_expired_msg: 'Your annual license has expired. Please enter a valid license key to reactivate the app.',
        license_expiring_soon_title: 'License Expiring Soon',
        license_expiring_soon_msg: 'Warning: Your license will expire in {0} day(s). Please renew your license key.',
        close_app: 'Exit Application',
        feature_scale_title: 'Hardware Scale',
        feature_scale_hint: 'Only Softio Super Admin can enable sell-by-weight products and the POS scale panel.',
        feature_scale_enable: 'Enable scale / sell-by-weight',
        feature_scale_on: 'Scale feature is ON — weight products and POS scale are available.',
        feature_scale_off: 'Scale feature is OFF.',
        feature_scale_saved: 'Scale feature updated',
        feature_scale_denied: 'Only Softio Super Admin can change this',
        feature_quicksale_title: 'Quick Sale',
        feature_quicksale_hint: 'Only Softio Super Admin can enable Quick Sale on the POS (sell without stock / custom items).',
        feature_quicksale_enable: 'Enable Quick Sale',
        feature_quicksale_on: 'Quick Sale is ON — the POS Quick Sale button is available.',
        feature_quicksale_off: 'Quick Sale is OFF.',
        feature_quicksale_saved: 'Quick Sale feature updated',
        feature_quicksale_denied: 'Only Softio Super Admin can change this',
        trial_ok: 'Trial started', trial_used: 'Trial already used', copied: 'Copied',
        cannot_delete_super_admin: 'Super Admin cannot be deleted',
        about_us: 'About Us', about_company: 'Softio', about_app_name: 'Al-Tayebat Inventory',
        about_version: 'Version 1.0.2 Platinum',
        about_desc: 'A comprehensive Inventory and Sales Management System designed to meet the needs of SMBs. Features modern UI, real-time sync, and multi-language support.',
        about_dev: 'Developed by Softio Digital Transformation',
        about_contact: 'Contact Support',
        about_copyright: '© 2026 Softio Services. All Rights Reserved.',
        adjust_stock: 'Adjust Stock', quote: 'Quote', draft: 'Draft', details: 'Details',
        receive_payment: 'Receive Payment', pay_supplier: 'Pay Supplier',
        supplier_debt_products: 'Add product',
        supplier_as_debt: 'Debt / Pay later',
        supplier_unadded_hint: 'Products from this supplier — click to fill, or use Import to inventory',
        supplier_mark_paid: 'Mark paid',
        supplier_mark_debt: 'Mark debt',
        supplier_added_inv: 'In inventory',
        supplier_not_added: 'Not in inventory',
        supplier_purchase_ok: 'Purchase line saved',
        supplier_from_supplier: 'From supplier',
        supplier_import_inv: 'Import to inventory',
        supplier_import_selected: 'Import selected',
        supplier_select_all: 'Select all',
        supplier_none_selected: 'Select at least one product',
        supplier_import_ok_created: 'Product created in inventory',
        supplier_import_ok_updated: 'Stock updated (qty added)',
        supplier_pick_supplier: 'Choose supplier',
        supplier_seed_demo: 'Load demo data',
        supplier_seed_ok: 'Demo data loaded',
        debt_unpaid_orders: 'Unpaid orders',
        debt_selected_total: 'Selected to pay',
        debt_clear_selection: 'Clear selection',
        debt_pay_order: 'Pay toward order',
        debt_remaining: 'Remaining',
        debt_no_orders: 'No unpaid orders for this customer.',
        debt_recent_payments: 'Recent payments',
        debt_select_or_amount: 'Select items/orders or enter an amount',
        debt_orders_owed: 'Open orders: {0}',
        manage_categories: 'Manage Categories', rename: 'Rename', stock_ok: 'Stock updated',
        payment_ok: 'Payment recorded', restore_backup: 'Restore', confirm_restore: 'Restore this backup? All current data will be replaced.',
        add_exp_category: 'Add Category', recurring: 'Recurring', one_time: 'One-time', recurring_monthly: 'Recurring monthly', add_expense_category_option: '+ Add new category', note: 'Note', walk_in: 'Walk-in',
        walk_in_customer: 'Walk-in Customer', new_order: 'New Order',
        vat_label: 'VAT (11%)', shipping: 'Shipping', discount: 'Discount (%)',
        total_payable: 'Total Payable', currency: 'Currency',
        return_items: 'Return', save_draft: 'Save Draft', quotation: 'Quotation',
        customer_bill: 'Pay Later', place_order: 'Pay Now',
        empty_cart_hint: 'Cart is empty! Please add items first.',
        pay_later_hint: 'Sell now, collect payment later',
        bill_need_customer: 'Select a customer for Pay Later',
        bill_ok: 'Added to customer debt',
        bill_ok_detail: 'Added {0} to {1}. Amount owed: {2}',
        pos_amount_owed: 'Amount owed',
        pos_credit_left: 'Credit left',
        credit_limit_warn: 'This sale would exceed the credit limit ({0}). New amount owed would be {1}. Continue?',
        filter_with_debt: 'With debt',
        print_receipt: 'Receipt',
        nav_pos_full: 'Point of Sale', manage_drafts: 'Manage Drafts',
        add_shipping: 'Add Shipping Details', view_shipping: 'View Shipping Details',
        pos_orders: 'Orders', pos_sales: 'Sales', pos_pending: 'Pending',
        shipping_to: 'Shipping To', order_date: 'Order Date', delivery_date: 'Delivery Date',
        payment_due: 'Payment Due Date', shipping_saved: 'Shipping details saved',
        delete_draft: 'Delete', load_draft: 'Open', no_drafts: 'No draft orders',
        bulk_delete: 'Bulk Delete', filter_all: 'All', filter_low_stock: 'Low Stock', filter_active: 'Active Only',
        col_image: 'Image', col_min_stock: 'Min Stock', col_location: 'Location', col_cost: 'Cost', col_supplier: 'Supplier',
        item_type: 'Type', type_product: 'Product', type_service: 'Service',
        sell_by: 'How is this sold?', sell_by_piece: 'Fixed price — piece, box, or pack', sell_by_weight: 'By weight — price per kilogram',
        sell_by_hint: 'Bulk chocolate: Price = $/kg. Stock in grams (5000 = 5 kg).',
        sell_by_hint_piece: 'Pick the unit below (pcs / box / pack). Price is for one unit; stock is how many you have.',
        weight_guide_title: 'Weight product setup',
        weight_guide_1: 'Set Price / kg for this chocolate type (example: 40 = $40 for 1 kg).',
        weight_guide_2: 'Set Stock (g) in grams (example: 5000 = 5 kg on the shelf, 1000 g = 1 kg).',
        weight_guide_3: 'At POS: Weigh → enter kg or Read scale → Add to cart.',
        price_per_kg: 'Price / kg', price_per_kg_hint: 'Enter the price of 1 kg. POS multiplies by the weighed amount.',
        stock_grams: 'Stock (g)', low_level_grams: 'Low level (g)',
        stock_grams_hint: 'Enter stock in grams (g). Details: 1000 g = 1 kg | 5000 g = 5 kg | 1250 g = 1.250 kg.',
        stock_units_hint: 'How many units on the shelf (pieces, boxes, or packs).',
        uom_piece_hint: 'pcs / box / pack — price is for one unit',
        uom_weight_hint: 'Locked to g (grams). Price is per kg; stock is entered in grams (g) (1000 g = 1 kg).',
        weigh_first: 'Product selected — enter kg or Read scale, then Add to cart',
        weigh_need_product: 'Press Weigh on a product first',
        weigh_need_scale: 'Enter a weight greater than 0',
        weigh_need_read: 'Enter weight (or Read scale) first',
        weigh_added: 'Added {0} @ {1} kg',
        weigh_btn: 'Weigh', weight_badge: 'By Weight',
        per_kg: '/kg',
        scale_pick_product: 'Press on a weight product below',
        scale_weight_lbl: 'Weight (g)', scale_price_lbl: 'Price',
        scale_read: 'Read scale', scale_add_cart: 'Add to cart',
        scale_selected: 'Selected',
        scale_manual_hint: 'Enter grams (g). Example: 1000 = 1.000 kg | 1250 = 1.250 kg',
        scale_offline_manual: 'Enter grams (g). Example: 1000 = 1.000 kg | 1250 = 1.250 kg',
        scale_offline: 'Scale offline',
        scale_online: 'Scale online',
        scale_unstable: 'Scale unstable',
        scale_settings: 'Scale settings',
        scale_clear: 'Remove from scale',
        scale_tare: 'Tare',
        scale_zero: 'Zero',
        scale_tare_hint: 'Ignore container weight',
        scale_zero_hint: 'Reset empty scale to zero',
        scale_port: 'Port',
        scale_baud: 'Baud',
        scale_auto_connect: 'Auto-connect',
        scale_connect: 'Connect',
        scale_disconnect: 'Disconnect',
        scale_sim: 'Sim',
        scale_sim_hint: 'Simulate 0.525 kg',
        unit_kg: 'kg',
        weight_in_kg: 'Weight in kg',
        scale_connected: 'Scale connected',
        scale_connect_failed: 'Scale connect failed',
        scale_disconnected: 'Scale disconnected',
        scale_api_unavailable: 'Scale API unavailable',
        scale_weighed_toast: '{0}: {1} {2} → {3}',
        scale_simulated: 'Simulated {0} {1}',
        scale_manual_set: 'Manual {0} {1}',
        upload_image: 'Upload Image', change_image: 'Change', remove_image: 'Remove',
        settings: 'Settings', sales_item: 'Sales item', purchase_item: 'Purchase item', inactive: 'Inactive',
        tax_rate: 'Tax Rate', expiry_date: 'Expiry Date', auto_sku: 'Auto', scan: 'Scan', batch_no: 'Batch No.',
        shelf: 'Shelf', uom: 'Unit of Measure', add_uom: 'Add Unit of Measure',
        add_uom_hint: 'Enter a custom unit name (e.g. carton, dozen).',
        uom_added: 'Unit added', uom_exists: 'Unit already exists', add: 'Add',
        stock_control: 'Stock control', track_stock: 'Track stock',
        low_level: 'Low level', prices: 'Prices', price_level: 'Level', gross_pct: 'Gross %',
        price1: 'Price 1', price2: 'Price 2', price3: 'Price 3', price4: 'Price 4',
        add_service: 'Add Service', edit_service: 'Edit Service', credit_limit: 'Credit Limit',
        reminder_days: 'Reminder Days', confirm_bulk_delete: 'Delete selected products?',
        card_view: 'Cards', table_view: 'Table', add_new: 'Add New',
        blind_return: 'Item Return', scan_or_search: 'Scan barcode or search...',
        confirm_clear_cart: 'Clear the current order?', draft_loaded: 'Draft loaded into cart',
        remove_line: 'Remove', invalid_price: 'Enter a valid price greater than zero',
        item_not_found: 'Item not found',
        return_ok_blind: 'Return processed',
        return_from_sale: 'Return from sale',
        return_from_sale_hint: 'Pick the customer, then the sale, then what to return.',
        quick_return: 'Quick return (no sale)',
        quick_return_hint: 'Use only when there is no receipt. Scan items to put back in stock.',
        show_sales: 'Show sales',
        back: 'Back',
        refund_total: 'Refund total',
        sold_qty: 'Sold',
        can_return: 'Can return',
        return_qty: 'Return qty',
        unit_price: 'Price',
        no_sales_for_customer: 'No sales found for this customer',
        select_customer_or_order: 'Select a customer or enter an order number',
        return_ok_detail: 'Returned {0} item(s). Refund {1}. Stock updated.',
        already_fully_returned: 'Nothing left to return on this sale',
        load_to_cart: 'Load',
        pos_menu: 'Menu', all_categories: 'All Categories', items_count: 'items',
        no_products: 'No products found.',
        scan_to_connect: 'Scan to Connect',
        copy_url: 'Copy URL',
        add_milk_intake: 'Record Milk Intake', filter_type: 'Expense Type', exp_filter_all: 'All (Milk + General)',
        exp_filter_milk: 'Raw Milk Only', exp_filter_general: 'General Expenses Only', all_deliverers: 'All Deliverers',
        settle_deliverer: 'Settle Deliverer', col_deliverer_details: 'Deliverer / Details', col_deliverer: 'Deliverer / Supplier',
        milk_type: 'Milk Type', milk_cow: 'Cow Milk', milk_sheep: 'Sheep Milk', milk_goat: 'Goat Milk',
        delivery_shift: 'Shift', shift_morning: 'Morning', shift_evening: 'Evening',
        milk_liters: 'Quantity (Liters / Kg)', price_per_liter: 'Price per Liter', col_payment_status: 'Payment Status',
        on_account_weekly: 'Unpaid / Weekly Account', paid_cash_now: 'Paid Cash Now', milk_weekly_statement: 'Weekly Milk Statement',
        rep_expenses_milk_analysis: 'Expenses & Milk Intake Analysis', rep_milk_deliverers: 'Milk by Deliverer',
        rep_general_expenses: 'Expenses by Category', col_deliveries: 'Deliveries', col_liters: 'Total Liters',
        col_avg_price: 'Avg Price/L', col_paid: 'Paid', col_unpaid: 'Unpaid Due',
        rep_milk_purchases: 'Raw Milk Purchases', rep_unpaid_milk: 'Unpaid Deliverers', unpaid_deliverers: 'Due to milk deliverers',
        settle_confirm: 'Mark all unpaid deliveries for {0} as paid?', settle_ok: 'Deliveries settled successfully',
        no_unpaid_deliveries: 'No unpaid deliveries found for this deliverer', print_statement: 'Statement',
        preset_today: 'Today', preset_this_week: 'This Week', preset_last_week: 'Last Week',
        preset_this_month: 'This Month', preset_all: 'All Time', preset_label: 'Preset',
        unassigned: 'Unassigned / Walk-in', milk_cost: 'Milk Cost', unpaid_milk_balance: 'Unpaid Milk Due',
        due_to_deliverers: 'Due to Deliverers', net_balance_due: 'Net Balance Due',
        deliverer_signature: 'Deliverer Signature', accountant_signature: 'Accountant / Receiver Signature',
        confirm_settle_deliverer: 'Are you sure you want to settle unpaid deliveries for this deliverer?', total_due: 'Total Due',
        mark_unpaid: 'Mark Unpaid', add_new_deliverer: '+ Add new deliverer / supplier',
        quick_add_deliverer_prompt: 'Enter deliverer or provider name (e.g. Ahmed, Youssef):', deliverer_added_ok: 'Deliverer / supplier added successfully'
    },
    ar: {
        login_title: 'تسجيل الدخول', login_subtitle: 'بوابة إدارة المخزون',
        username: 'اسم المستخدم', password: 'كلمة المرور', login_btn: 'دخول', logout: 'خروج',
        nav_dashboard: 'لوحة التحكم', nav_inventory: 'المخزون', nav_pos: 'نقطة البيع', nav_sales: 'المبيعات',
        nav_reports: 'التقارير', nav_history: 'السجل', nav_quotations: 'عروض الأسعار',
        nav_customers: 'العملاء', nav_suppliers: 'الموردون', nav_expenses: 'المصروفات',
        nav_currencies: 'العملات', nav_barcodes: 'الباركود', nav_users: 'المستخدمون', nav_settings: 'الإعدادات',
        dash_subtitle: 'نظرة عامة على نشاطك', inv_subtitle: 'إدارة القطع ومستويات المخزون',
        pos_subtitle: 'امسح أو اضغط على المنتجات للبيع', sales_subtitle: 'سجل الطلبات والمرتجعات',
        reports_subtitle: 'ملخص المبيعات والأرباح', history_subtitle: 'سجل النشاط',
        quot_subtitle: 'عروض بانتظار التحويل', cust_subtitle: 'دليل العملاء',
        supp_subtitle: 'دليل الموردين', exp_subtitle: 'تتبع مصروفات العمل',
        cur_subtitle: 'أسعار الصرف مقابل الدولار', bar_subtitle: 'معاينة ملصقات الطباعة',
        barcode_preview_title: 'طباعة',
        select_all: 'تحديد الكل',
        select_barcodes_first: 'حدد باركوداً واحداً على الأقل للطباعة',
        print_destination: 'الوجهة',
        print_copies: 'النسخ',
        print_layout: 'الاتجاه',
        print_portrait: 'عمودي',
        print_landscape: 'أفقي',
        print_pages: 'الصفحات',
        print_custom: 'مخصص',
        print_pages_ph: 'مثال: 1-5, 8',
        print_color: 'اللون',
        print_color_full: 'ملون',
        print_color_bw: 'أبيض وأسود',
        print_sheet_one: 'الإجمالي: ورقة واحدة',
        print_sheet_many: 'الإجمالي: {0} أوراق',
        print_system_printer: 'طابعة النظام…',
        print_sent: 'تم الإرسال للطابعة',
        print_failed: 'فشلت الطباعة',
        print_settings_title: 'الطابعة وحجم الورق',
        print_settings_hint: 'اضبط حجم الورق بالمليمتر لأي طابعة. كل طابعة تحتفظ بحجمها.',
        print_label_size: 'ملصقات الباركود',
        print_receipt_size: 'إيصال نقطة البيع',
        print_preset: 'إعداد مسبق',
        print_width_mm: 'العرض (مم)',
        print_height_mm: 'الارتفاع (مم)',
        print_label_w: 'عرض الملصق (مم)',
        print_label_h: 'ارتفاع الملصق (مم)',
        print_page_w: 'عرض الصفحة (مم)',
        print_page_h: 'ارتفاع الصفحة (مم)',
        print_gap_mm: 'المسافة (مم)',
        print_margin_mm: 'الهامش (مم)',
        print_margin_top: 'أعلى (مم)',
        print_margin_right: 'يمين (مم)',
        print_margin_bottom: 'أسفل (مم)',
        print_margin_left: 'يسار (مم)',
        print_columns: 'الأعمدة',
        print_columns_auto: 'تلقائي',
        print_default_label_printer: 'طابعة الملصقات الافتراضية',
        print_default_receipt_printer: 'طابعة الإيصال الافتراضية',
        print_profiles_title: 'ملفات الطابعات المحفوظة',
        print_profiles_hint: 'يتم حفظ الحجم لكل طابعة. احذف ملفاً لإعادة ضبط الجهاز.',
        print_profiles_clear: 'مسح كل الملفات',
        print_profiles_empty: 'لا توجد ملفات طابعات محفوظة بعد.',
        print_profile_remove: 'حذف',
        print_paper_mode: 'وضع الورق',
        print_mode_sheet: 'ورقة (عدة ملصقات)',
        print_mode_roll: 'طابعة ملصقات (ملصق واحد)',
        print_settings_save: 'حفظ إعدادات الطباعة',
        print_settings_saved: 'تم حفظ إعدادات الطباعة',
        print_settings_fail: 'تعذر حفظ إعدادات الطباعة',
        users_subtitle: 'الحسابات والصلاحيات', settings_subtitle: 'اللغة والترخيص والمظهر',
        settings_general: 'عام', settings_language_hint: 'اختر لغة واجهة التطبيق.',
        recent_sales: 'أحدث المبيعات', add_product: 'إضافة منتج', add_expense: 'إضافة مصروف',
        search_ph: 'بحث...', cart_title: 'السلة', clear: 'مسح', subtotal: 'المجموع الفرعي', total: 'الإجمالي',
        checkout: 'إتمام الدفع', cancel: 'إلغاء', save: 'حفظ', print: 'طباعة', convert: 'تأكيد',
        process_return: 'معالجة مرتجع', return_reason: 'السبب', return_reason_ph: 'مثال: تالف',
        confirm_return: 'تأكيد المرتجع', language: 'اللغة', license_info: 'الترخيص',
        top_products: 'أفضل المنتجات', best_selling: 'الأكثر مبيعاً', top_categories: 'أفضل الفئات', mark_paid: 'تعليم الدفع', paid: 'مدفوع', unpaid: 'غير مدفوع',
        preset_daily: 'يومي', preset_weekly: 'أسبوعي', preset_monthly: 'شهري', preset_yearly: 'سنوي', preset_custom: 'مخصص',
        date_from: 'من', date_to: 'إلى',
        col_order: 'الطلب', col_date: 'التاريخ', col_customer: 'العميل', col_total: 'الإجمالي',
        col_name: 'الاسم', col_sku: 'الرمز', col_category: 'الفئة', col_price: 'السعر',
        col_stock: 'المخزون', col_status: 'الحالة', col_actions: 'إجراءات', col_phone: 'الهاتف',
        col_email: 'البريد', col_balance: 'المبلغ المستحق', col_payment: 'الدفع', col_type: 'النوع', col_contact: 'جهة الاتصال',
        col_role: 'الدور', col_barcode: 'الباركود', col_qty: 'الكمية', col_sales: 'المبيعات',
        role_admin: 'مدير', role_staff: 'موظف', role_accountant: 'محاسب',
        col_profit: 'الربح', col_amount: 'المبلغ', col_desc: 'الوصف', col_code: 'الرمز',
        col_symbol: 'الرمز', col_rate: 'السعر',
        col_items: 'العناصر', col_order_id: 'رقم الطلب',
        all: 'الكل', in_stock: 'متوفر', low_stock: 'منخفض', out_of_stock: 'نفد',
        pos_out_of_stock_title: 'نفد المخزون',
        pos_out_of_stock_msg: '"{0}" نفد من المخزون.',
        pos_insufficient_stock_title: 'المخزون غير كافٍ',
        pos_insufficient_weight_msg: 'الكمية المطلوبة من "{0}" غير متوفرة بالكامل في المخزون.\n\nالوزن المطلوب: {1} كغ\nالمتوفر في المخزن: {2} كغ',
        pos_sell_anyway: 'بيع بدون خصم مخزون',
        quick_sale: 'بيع سريع',
        quick_sale_hint: 'بيع دون إضافة للمخزون، أو تسجيل صنف لمرة واحدة.',
        quick_sale_existing: 'منتج موجود',
        quick_sale_custom: 'صنف مخصص',
        quick_sale_skip_stock: 'لا تخصم من المخزون',
        quick_sale_badge: 'بدون مخزون',
        add_to_cart: 'أضف للسلة',
        qs_pick_product: 'اختر منتجاً',
        qs_need_name: 'أدخل اسم الصنف',
        qs_need_price: 'أدخل سعراً صالحاً',
        ok: 'حسناً',
        return: 'مرتجع', empty_cart: 'السلة فارغة', empty_list: 'لا توجد سجلات', loading: 'جاري التحميل…',
        login_failed: 'اسم المستخدم أو كلمة المرور غير صحيحة', checkout_ok: 'تمت عملية البيع',
        checkout_fail: 'فشل الدفع', product_ok: 'تمت إضافة المنتج', return_ok: 'تمت معالجة المرتجع',
        expense_ok: 'تم حفظ المصروف', convert_ok: 'تم التأكيد وتحويل العرض إلى طلب',
        today_sales: 'مبيعات اليوم', inventory_value: 'قيمة المخزون', total_items: 'إجمالي القطع',
        low_stock_count: 'مخزون منخفض', orders_today: 'طلبات اليوم',
        rep_sales: 'المبيعات', rep_cost: 'التكلفة', rep_expenses: 'المصروفات', rep_profit: 'الربح',
        rep_profit_before_expenses: 'الربح (قبل المصروفات)', rep_profit_after_expenses: 'الربح (بعد المصروفات)',
        lic_type: 'النوع', lic_customer: 'العميل', lic_expires: 'ينتهي', lic_days: 'الأيام المتبقية',
        lic_valid: 'صالح', lic_invalid: 'غير صالح', lic_trial: 'تجريبي', lic_machine: 'الجهاز',
        export: 'تصدير', tool_notif: 'الإشعارات', tool_lock: 'قفل', tool_calc: 'آلة حاسبة',
        refresh: 'تحديث', add_category: 'إضافة فئة', add_customer: 'إضافة عميل',
        add_supplier: 'إضافة مورد', add_user: 'إضافة مستخدم', add_currency: 'إضافة عملة',
        edit: 'تعديل', delete: 'حذف', confirm_delete: 'حذف هذا السجل؟', confirm_title: 'يرجى التأكيد', confirm_btn: 'تأكيد', import: 'استيراد',
        refresh_rates: 'تحديث الأسعار', view: 'عرض', deleted_ok: 'تم الحذف', saved_ok: 'تم الحفظ', import_ok: 'تم الاستيراد',
        view_order: 'عرض الطلب', col_address: 'العنوان', edit_product: 'تعديل المنتج',
        quote_preview_title: 'معاينة عرض السعر', quote_preview_quote: 'عرض سعر',
        quote_preview_cust_header: 'تفاصيل العميل', quote_col_photo: 'صورة', quote_col_desc: 'وصف الصنف', quote_col_qty: 'الكمية',
        quote_preview_meta: 'رقم العرض: {0}   |   التاريخ: {1}   |   رقم العميل: {2}   |   الصلاحية: {3}',
        quote_validity_days: '15 يوماً', quote_no_address: 'لا يوجد عنوان', quote_no_phone: 'لا يوجد هاتف',
        quote_terms_head: 'الشروط والأحكام',
        quote_terms_body: '• الصلاحية: 15 يوماً من تاريخ الإصدار.\n• الدفع مستحق قبل التسليم.\n• القبول يعني تأكيد الفوترة.\n\nتم القبول بواسطة: __________________________',
        quote_tax_extras: 'ضريبة / إضافات', grand_total: 'الإجمالي النهائي', preview: 'معاينة',
        tool_backup: 'نسخ احتياطي', tool_about: 'حول', backup_now: 'إنشاء نسخة', factory_reset: 'حذف المصنع', confirm_factory_1: 'حذف كل البيانات وإعادة ضبط قاعدة البيانات؟ لا يمكن التراجع.', confirm_factory_2: 'تأكيد أخير: مسح كل شيء والبدء من صفر؟', factory_ok: 'تمت إعادة ضبط قاعدة البيانات', factory_fail: 'فشل حذف المصنع', backup_export_ok: 'تم تصدير النسخة', backup_import_ok: 'تم استيراد النسخة',
        open_backup_folder: 'فتح المجلد', backup_ok: 'تم إنشاء النسخة', backup_fail: 'فشل النسخ',
        auto_backup: 'نسخ احتياطي تلقائي', auto_backup_off: 'إيقاف', auto_backup_daily: 'يومي',
        auto_backup_weekly: 'أسبوعي', auto_backup_monthly: 'شهري',
        auto_backup_hint_off: 'النسخ الاحتياطي التلقائي متوقف.',
        auto_backup_hint_on: 'يتم النسخ التلقائي أثناء تشغيل التطبيق.',
        backup_location: 'موقع النسخ', backup_choose_folder: 'اختيار مجلد', backup_use_default: 'المجلد الافتراضي',
        no_notifications: 'لا توجد إشعارات', clear_all_notifications: 'مسح الكل',
        clear_notification: 'مسح', confirm_clear_notifications: 'مسح كل الإشعارات؟',
        app_refreshed: 'تم تحديث التطبيق',
        locked_title: 'الشاشة مقفلة',
        locked_subtitle: 'أدخل كلمة المرور للمتابعة', unlock_btn: 'فتح القفل',
        unlock_fail: 'كلمة المرور غير صحيحة', about_blurb: 'نظام إدارة المخزون لشركة الطيبات.',
        last_backup: 'آخر نسخة', backup_none: 'لا توجد نسخة بعد',
        activate_license: 'تفعيل الترخيص', activate_btn: 'تفعيل', start_trial: 'بدء التجربة',
        license_key: 'مفتاح الترخيص', machine_id: 'معرّف الجهاز', copy: 'نسخ',
        license_subtitle: 'أدخل مفتاح الترخيص لتفعيل هذا الجهاز.',
        license_ok: 'تم تفعيل الترخيص بنجاح', license_invalid: 'مفتاح الترخيص غير صالح',
        trial_expired_msg: 'انتهت الفترة التجريبية. يرجى إدخال مفتاح ترخيص صالح لتفعيل التطبيق.',
        license_expired_msg: 'انتهت صلاحية الترخيص السنوي. يرجى إدخال مفتاح ترخيص صالح لإعادة تفعيل التطبيق.',
        license_expiring_soon_title: 'الترخيص سينتهي قريباً',
        license_expiring_soon_msg: 'تحذير: سينتهي ترخيصك خلال {0} أيام. يرجى تجديد مفتاح الترخيص.',
        close_app: 'إغلاق التطبيق',
        feature_scale_title: 'ميزان الأجهزة',
        feature_scale_hint: 'يمكن لمشرف Softio فقط تفعيل البيع بالوزن ولوحة الميزان في نقطة البيع.',
        feature_scale_enable: 'تفعيل الميزان / البيع بالوزن',
        feature_scale_on: 'ميزة الميزان مفعّلة — منتجات الوزن وميزان نقطة البيع متاحة.',
        feature_scale_off: 'ميزة الميزان مطفأة.',
        feature_scale_saved: 'تم تحديث ميزة الميزان',
        feature_scale_denied: 'يمكن لمشرف Softio فقط تغيير هذا الإعداد',
        feature_quicksale_title: 'بيع سريع',
        feature_quicksale_hint: 'يمكن لمشرف Softio فقط تفعيل البيع السريع في نقطة البيع (بيع دون مخزون / أصناف مخصصة).',
        feature_quicksale_enable: 'تفعيل البيع السريع',
        feature_quicksale_on: 'البيع السريع مفعّل — زر البيع السريع متاح في نقطة البيع.',
        feature_quicksale_off: 'البيع السريع مطفأ.',
        feature_quicksale_saved: 'تم تحديث ميزة البيع السريع',
        feature_quicksale_denied: 'يمكن لمشرف Softio فقط تغيير هذا الإعداد',
        trial_ok: 'تم بدء الفترة التجريبية', trial_used: 'تم استخدام التجربة مسبقاً', copied: 'تم النسخ',
        cannot_delete_super_admin: 'لا يمكن حذف المسؤول الأعلى',
        about_us: 'من نحن', about_company: 'Softio', about_app_name: 'نظام مخزون الطيبات', about_version: 'الإصدار 1.0.2 Platinum',
        about_desc: 'نظام متكامل لإدارة المخازن والمبيعات، مصمم خصيصاً لتلبية احتياجات الشركات الصغيرة والمتوسطة. يتميز بواجهة عصرية ودعم كامل للغة العربية.',
        about_dev: 'تطوير بواسطة Softio Digital Transformation', about_contact: 'تواصل معنا',
        about_copyright: '© 2026 Softio Services. جميع الحقوق محفوظة.',
        adjust_stock: 'تعديل المخزون', quote: 'عرض سعر', draft: 'مسودة', details: 'التفاصيل',
        receive_payment: 'استلام دفعة', pay_supplier: 'دفع للمورد',
        supplier_debt_products: 'إضافة منتج',
        supplier_as_debt: 'دين / دفع لاحقاً',
        supplier_unadded_hint: 'منتجات هذا المورد — اضغط للتعبئة أو استورد للمخزون',
        supplier_mark_paid: 'تعيين كمدفوع',
        supplier_mark_debt: 'تعيين كدين',
        supplier_added_inv: 'في المخزون',
        supplier_not_added: 'ليس في المخزون',
        supplier_purchase_ok: 'تم حفظ بند الشراء',
        supplier_from_supplier: 'من المورد',
        supplier_import_inv: 'استيراد للمخزون',
        supplier_import_selected: 'استيراد المحدد',
        supplier_select_all: 'تحديد الكل',
        supplier_none_selected: 'حدد منتجاً واحداً على الأقل',
        supplier_import_ok_created: 'تم إنشاء المنتج في المخزون',
        supplier_import_ok_updated: 'تم تحديث الكمية في المخزون',
        supplier_pick_supplier: 'اختر المورد',
        supplier_seed_demo: 'تحميل بيانات تجريبية',
        supplier_seed_ok: 'تم تحميل البيانات التجريبية',
        debt_unpaid_orders: 'طلبات غير مدفوعة',
        debt_selected_total: 'المبلغ المحدد',
        debt_clear_selection: 'مسح التحديد',
        debt_pay_order: 'دفع على الطلب',
        debt_remaining: 'المتبقي',
        debt_no_orders: 'لا توجد طلبات غير مدفوعة لهذا العميل.',
        debt_recent_payments: 'الدفعات الأخيرة',
        debt_select_or_amount: 'حدد أصنافاً/طلبات أو أدخل مبلغاً',
        debt_orders_owed: 'طلبات مفتوحة: {0}',
        manage_categories: 'إدارة الفئات', rename: 'إعادة تسمية', stock_ok: 'تم تحديث المخزون',
        payment_ok: 'تم تسجيل الدفعة', restore_backup: 'استعادة', confirm_restore: 'استعادة هذه النسخة؟ سيتم استبدال جميع البيانات الحالية.',
        add_exp_category: 'إضافة فئة', recurring: 'متكرر', one_time: 'مرة واحدة', recurring_monthly: 'مصروف شهري متكرر', add_expense_category_option: '+ إضافة فئة جديدة', note: 'ملاحظة', walk_in: 'عميل عابر',
        walk_in_customer: 'عميل عابر', new_order: 'طلب جديد',
        vat_label: 'ضريبة (11%)', shipping: 'الشحن', discount: 'خصم (%)',
        total_payable: 'الإجمالي المستحق', currency: 'العملة',
        return_items: 'مرتجع', save_draft: 'حفظ مسودة', quotation: 'عرض سعر',
        customer_bill: 'ادفع لاحقاً', place_order: 'ادفع الآن',
        empty_cart_hint: 'السلة فارغة! أضف منتجات أولاً.',
        pay_later_hint: 'بيع الآن واستلام الدفع لاحقاً',
        bill_need_customer: 'اختر عميلاً لـ ادفع لاحقاً',
        bill_ok: 'أُضيف إلى دين العميل',
        bill_ok_detail: 'أُضيف {0} إلى {1}. المبلغ المستحق: {2}',
        pos_amount_owed: 'المبلغ المستحق',
        pos_credit_left: 'الائتمان المتبقي',
        credit_limit_warn: 'هذه العملية تتجاوز حد الائتمان ({0}). المبلغ المستحق الجديد سيكون {1}. المتابعة؟',
        filter_with_debt: 'عليهم دين',
        print_receipt: 'إيصال',
        nav_pos_full: 'نقطة البيع', manage_drafts: 'إدارة المسودات',
        add_shipping: 'إضافة تفاصيل الشحن', view_shipping: 'عرض تفاصيل الشحن',
        pos_orders: 'الطلبات', pos_sales: 'المبيعات', pos_pending: 'قيد الانتظار',
        shipping_to: 'الشحن إلى', order_date: 'تاريخ الطلب', delivery_date: 'تاريخ التسليم',
        payment_due: 'تاريخ الاستحقاق', shipping_saved: 'تم حفظ تفاصيل الشحن',
        delete_draft: 'حذف', load_draft: 'فتح', no_drafts: 'لا توجد مسودات',
        bulk_delete: 'حذف جماعي', filter_all: 'الكل', filter_low_stock: 'مخزون منخفض', filter_active: 'النشط فقط',
        col_image: 'الصورة', col_min_stock: 'الحد الأدنى', col_location: 'الموقع', col_cost: 'التكلفة', col_supplier: 'المورد',
        upload_image: 'رفع صورة', change_image: 'تغيير', remove_image: 'إزالة', item_type: 'النوع', type_product: 'منتج', type_service: 'خدمة',
        sell_by: 'كيف يُباع؟', sell_by_piece: 'سعر ثابت — قطعة أو علبة أو عبوة', sell_by_weight: 'بالوزن — السعر لكل كيلوغرام',
        sell_by_hint: 'شوكولا بالوزن: السعر = $/كغ. المخزون بالغرام (5000 = 5 كغ).',
        sell_by_hint_piece: 'اختر الوحدة أدناه (قطعة / علبة / عبوة). السعر لوحدة واحدة؛ المخزون = الكمية المتوفرة.',
        weight_guide_title: 'إعداد منتج بالوزن',
        weight_guide_1: 'ضع السعر / كغ لهذا النوع (مثال: 40 = 40$ لكل 1 كغ).',
        weight_guide_2: 'ضع المخزون (غ) بالغرام (مثال: 5000 = 5 كغ على الرف، 1000 غ = 1 كغ).',
        weight_guide_3: 'في نقطة البيع: وزن → أدخل الكغ أو اقرأ الميزان → أضف للسلة.',
        price_per_kg: 'السعر / كغ', price_per_kg_hint: 'أدخل سعر 1 كغ. نقطة البيع تضربه بالوزن.',
        stock_grams: 'المخزون (غ)', low_level_grams: 'الحد الأدنى (غ)',
        stock_grams_hint: 'أدخل المخزون بالغرام (غ). التفاصيل: 1000 غ = 1 كغ | 5000 غ = 5 كغ | 1250 غ = 1.250 كغ.',
        stock_units_hint: 'عدد الوحدات على الرف (قطع أو علب أو عبوات).',
        uom_piece_hint: 'قطعة / علبة / عبوة — السعر لوحدة واحدة',
        uom_weight_hint: 'مثبت على غرام (غ). السعر لكل كغ؛ المخزون أدناه بالغرام (غ) (1000 غ = 1 كغ).',
        weigh_first: 'تم اختيار المنتج — أدخل الكغ أو اقرأ الميزان ثم أضف للسلة',
        weigh_need_product: 'اضغط وزن على منتج أولاً',
        weigh_need_scale: 'أدخل وزناً أكبر من 0',
        weigh_need_read: 'أدخل الوزن (أو اقرأ الميزان) أولاً',
        weigh_added: 'أُضيف {0} @ {1} كغ',
        weigh_btn: 'وزن', weight_badge: 'بالوزن',
        per_kg: '/كغ',
        scale_pick_product: 'اضغط على منتج بالوزن من الأسفل',
        scale_weight_lbl: 'الوزن (غ)', scale_price_lbl: 'السعر',
        scale_read: 'قراءة الميزان', scale_add_cart: 'إضافة للسلة',
        scale_selected: 'المحدد',
        scale_manual_hint: 'أدخل الغرام (غ). مثال: 1000 = 1.000 كغ | 1250 = 1.250 كغ',
        scale_offline_manual: 'أدخل الغرام (غ). مثال: 1000 = 1.000 كغ | 1250 = 1.250 كغ',
        scale_offline: 'الميزان غير متصل',
        scale_online: 'الميزان متصل',
        scale_unstable: 'الميزان غير مستقر',
        scale_settings: 'إعدادات الميزان',
        scale_clear: 'إزالة من الميزان',
        scale_tare: 'تارا',
        scale_zero: 'تصفير',
        scale_tare_hint: 'تجاهل وزن الوعاء',
        scale_zero_hint: 'إعادة الميزان الفارغ إلى الصفر',
        scale_port: 'المنفذ',
        scale_baud: 'معدل الباود',
        scale_auto_connect: 'اتصال تلقائي',
        scale_connect: 'اتصال',
        scale_disconnect: 'قطع الاتصال',
        scale_sim: 'محاكاة',
        scale_sim_hint: 'محاكاة 0.525 كغ',
        unit_kg: 'كغ',
        weight_in_kg: 'الوزن بالكغ',
        scale_connected: 'تم الاتصال بالميزان',
        scale_connect_failed: 'فشل الاتصال بالميزان',
        scale_disconnected: 'تم قطع اتصال الميزان',
        scale_api_unavailable: 'واجهة الميزان غير متاحة',
        scale_weighed_toast: '{0}: {1} {2} ← {3}',
        scale_simulated: 'تمت محاكاة {0} {1}',
        scale_manual_set: 'يدوي {0} {1}',
        settings: 'الإعدادات', sales_item: 'عنصر مبيعات', purchase_item: 'عنصر مشتريات', inactive: 'غير نشط',
        tax_rate: 'نسبة الضريبة', expiry_date: 'تاريخ الانتهاء', auto_sku: 'تلقائي', scan: 'مسح', batch_no: 'رقم الدفعة',
        shelf: 'الرف', uom: 'وحدة القياس', add_uom: 'إضافة وحدة قياس',
        add_uom_hint: 'أدخل اسم وحدة مخصصة (مثل كرتون، دستة).',
        uom_added: 'تمت إضافة الوحدة', uom_exists: 'الوحدة موجودة مسبقاً', add: 'إضافة',
        stock_control: 'التحكم بالمخزون', track_stock: 'تتبع المخزون',
        low_level: 'الحد الأدنى', prices: 'الأسعار', price_level: 'المستوى', gross_pct: 'الإجمالي %',
        price1: 'السعر 1', price2: 'السعر 2', price3: 'السعر 3', price4: 'السعر 4',
        add_service: 'إضافة خدمة', edit_service: 'تعديل الخدمة', credit_limit: 'حد الائتمان',
        reminder_days: 'أيام التذكير', confirm_bulk_delete: 'حذف المنتجات المحددة؟',
        card_view: 'بطاقات', table_view: 'جدول', add_new: 'إضافة جديد',
        blind_return: 'مرتجع أصناف', scan_or_search: 'امسح الباركود أو ابحث...',
        confirm_clear_cart: 'مسح الطلب الحالي؟', draft_loaded: 'تم تحميل المسودة إلى السلة',
        remove_line: 'إزالة', invalid_price: 'أدخل سعراً صالحاً أكبر من صفر',
        item_not_found: 'الصنف غير موجود',
        return_ok_blind: 'تمت معالجة المرتجع',
        return_from_sale: 'مرتجع من فاتورة',
        return_from_sale_hint: 'اختر العميل ثم الفاتورة ثم الأصناف المراد إرجاعها.',
        quick_return: 'مرتجع سريع (بدون فاتورة)',
        quick_return_hint: 'استخدمه فقط عند عدم وجود إيصال. امسح الأصناف لإعادتها للمخزون.',
        show_sales: 'عرض المبيعات',
        back: 'رجوع',
        refund_total: 'إجمالي الاسترداد',
        sold_qty: 'مباع',
        can_return: 'قابل للإرجاع',
        return_qty: 'كمية الإرجاع',
        unit_price: 'السعر',
        no_sales_for_customer: 'لا توجد مبيعات لهذا العميل',
        select_customer_or_order: 'اختر عميلاً أو أدخل رقم الطلب',
        return_ok_detail: 'تم إرجاع {0} صنف/أصناف. الاسترداد {1}. تم تحديث المخزون.',
        already_fully_returned: 'لا يوجد شيء متبقٍ للإرجاع في هذه الفاتورة',
        load_to_cart: 'تحميل',
        pos_menu: 'القائمة', all_categories: 'كل الفئات', items_count: 'أصناف',
        no_products: 'لا توجد منتجات.',
        scan_to_connect: 'امسح للاتصال',
        scan_to_connect_hint: 'نفس الواي فاي — افتح تطبيق الطيبات بالكامل على الهاتف أو الجهاز اللوحي.',
        copy_url: 'نسخ الرابط',
        add_milk_intake: 'تسجيل استلام حليب', filter_type: 'نوع المصروف', exp_filter_all: 'الكل (حليب + عام)',
        exp_filter_milk: 'حليب خام فقط', exp_filter_general: 'مصاريف عامة فقط', all_deliverers: 'جميع الموردين',
        settle_deliverer: 'تسوية حساب المورّد', col_deliverer_details: 'المورّد / التفاصيل', col_deliverer: 'المورّد',
        milk_type: 'نوع الحليب', milk_cow: 'حليب بقر', milk_sheep: 'حليب غنم', milk_goat: 'حليب ماعز',
        delivery_shift: 'الوجبة / الوردية', shift_morning: 'صباحي', shift_evening: 'مسائي',
        milk_liters: 'الكمية (لتر / كغ)', price_per_liter: 'سعر اللتر', col_payment_status: 'حالة الدفع',
        on_account_weekly: 'على الحساب (أسبوعي)', paid_cash_now: 'مدفوع نقداً', milk_weekly_statement: 'كشف حساب الحليب الأسبوعي',
        rep_expenses_milk_analysis: 'تحليل المصاريف وتوريد الحليب', rep_milk_deliverers: 'الحليب حسب المورّد',
        rep_general_expenses: 'المصاريف حسب البند', col_deliveries: 'التوريدات', col_liters: 'إجمالي اللترات',
        col_avg_price: 'متوسط سعر اللتر', col_paid: 'المدفوع', col_unpaid: 'المتبقي / مستحق',
        rep_milk_purchases: 'مشتريات الحليب الخام', rep_unpaid_milk: 'مستحقات الموردين', unpaid_deliverers: 'مستحقات غير مدفوعة لموردي الحليب',
        settle_confirm: 'تسوية وتسجيل جميع توريدات الحليب غير المدفوعة للمورّد {0} كمدفوعة؟', settle_ok: 'تمت تسوية حساب المورّد بنجاح',
        no_unpaid_deliveries: 'لا توجد توريدات غير مدفوعة لهذا المورّد', print_statement: 'كشف حساب',
        preset_today: 'اليوم', preset_this_week: 'هذا الأسبوع', preset_last_week: 'الأسبوع الماضي',
        preset_this_month: 'هذا الشهر', preset_all: 'كل الأوقات', preset_label: 'الفترة',
        unassigned: 'غير محدد / مباشر', milk_cost: 'تكلفة الحليب', unpaid_milk_balance: 'مستحقات حليب غير مدفوعة',
        due_to_deliverers: 'مستحقات لموردي الحليب', net_balance_due: 'صافي الرصيد المستحق',
        deliverer_signature: 'توقيع المورّد', accountant_signature: 'توقيع المحاسب / المستلم',
        confirm_settle_deliverer: 'هل أنت متأكد من تسوية التوريدات غير المدفوعة لهذا المورّد؟', total_due: 'إجمالي المستحق',
        mark_unpaid: 'تسجيل كغير مدفوع', add_new_deliverer: '+ إضافة مورّد / سائق جديد',
        quick_add_deliverer_prompt: 'أدخل اسم المورّد أو السائق أو جهة الصرف (مثال: أحمد، يوسف):', deliverer_added_ok: 'تمت إضافة المورّد / جهة الصرف بنجاح'
    }
};

let lang = localStorage.getItem('generic_lang')   || 'en';
let currentUser = null;
let featureFlags = { scaleEnabled: false, quickSaleEnabled: false };
let printSettings = {
    labelWidthMm: 60,
    labelHeightMm: 36,
    labelGapMm: 5,
    labelMarginMm: 2,
    labelMarginTopMm: 2,
    labelMarginRightMm: 2,
    labelMarginBottomMm: 2,
    labelMarginLeftMm: 2,
    labelColumns: 0,
    labelPaperMode: 'sheet',
    labelPageWidthMm: 210,
    labelPageHeightMm: 297,
    receiptWidthMm: 80,
    receiptHeightMm: 0,
    receiptMarginMm: 2.5,
    receiptMarginTopMm: 2.5,
    receiptMarginRightMm: 2.5,
    receiptMarginBottomMm: 2.5,
    receiptMarginLeftMm: 2.5,
    labelPrinter: '',
    receiptPrinter: '',
    printerProfiles: {}
};
const LABEL_PRESETS = {
    '60x36': { w: 60, h: 36 },
    '40x30': { w: 40, h: 30 },
    '50x25': { w: 50, h: 25 },
    '60x40': { w: 60, h: 40 },
    '100x50': { w: 100, h: 50 }
};
let products = [], categories = [], sales = [], customers = [], suppliers = [], users = [];
let expenses = [], quotations = [], currencies = [], barcodeItems = [], expenseCategories = [];
let dashboard = null, reportSummary = null, reportTop = [], reportExpensesBreakdown = { milkSuppliers: [], categories: [] };
let cart = [], invCat = 'all', posCat = 'all', invFilter = 'all', invView = 'card';
let invSelected = new Set();
let barcodeSelected = new Set();
let returnOrderId = null, returnItemsCache = [];
let blindReturnItems = [];
let posReturnOrderId = null;
let posReturnItemsCache = [];
let editingProductId = null;
let posShipping = null;
let lastTappedProductId = null;
let scaleState = { connected: false, weight: 0, unit: 'kg', stable: true, port: '' };
/** Coalesce overlapping reloads so a slow fetch cannot overwrite a newer one. */
let _loadDataInflight = null;
let _loadDataAgain = false;
/** Ignore SignalR InventoryChanged briefly after local save (avoids double-fetch race). */
let _suppressSignalRReloadUntil = 0;
let _connectQrObjectUrl = null;
const QR_PLACEHOLDER =
    'data:image/svg+xml,' + encodeURIComponent(
        '<svg xmlns="http://www.w3.org/2000/svg" width="120" height="120" viewBox="0 0 120 120">' +
        '<rect width="120" height="120" fill="#f3f4f6"/>' +
        '<rect x="18" y="18" width="28" height="28" fill="#9ca3af"/>' +
        '<rect x="74" y="18" width="28" height="28" fill="#9ca3af"/>' +
        '<rect x="18" y="74" width="28" height="28" fill="#9ca3af"/>' +
        '<rect x="52" y="52" width="16" height="16" fill="#9ca3af"/>' +
        '</svg>');
const POS_VAT_RATE = 0.11;

function productPriceTiers(p) {
    if (!p) return [0];
    const tiers = [Number(p.price) || 0, Number(p.price2) || 0, Number(p.price3) || 0, Number(p.price4) || 0];
    return tiers;
}

function isSellByWeight(p) {
    if (!p) return false;
    const flag = p.sellByWeight ?? p.sell_by_weight;
    if (flag === true || flag === 1 || flag === '1') return true;
    const uom = String(p.uom || '').toLowerCase();
    const cat = String(p.category || '').toLowerCase();
    if (['kg', 'g', 'gram', 'grams', 'kilo', 'kilos', 'kilogram', 'kilograms', 'lb', 'lbs', 'oz'].includes(uom)) return true;
    if (cat.includes('bulk') || cat.includes('weight') || cat.includes('وزن') || cat.includes('فرل') || cat.includes('فله')) return true;
    return false;
}

function isSoftioSuperAdmin() {
    return !!(currentUser?.isSoftioSuperAdmin
        || (currentUser?.username || '').toLowerCase() === 'softio.admin');
}

function applyFeatureFlags(flags) {
    featureFlags = {
        scaleEnabled: !!(flags?.scaleEnabled ?? flags?.ScaleEnabled),
        quickSaleEnabled: !!(flags?.quickSaleEnabled ?? flags?.QuickSaleEnabled)
    };
    document.body.classList.toggle('feature-scale', featureFlags.scaleEnabled);
    document.body.classList.toggle('feature-quicksale', featureFlags.quickSaleEnabled);

    const scalePanel = document.getElementById('scalePanel');
    if (scalePanel) scalePanel.hidden = false;

    const sellFs = document.getElementById('p-sell-by-fieldset');
    if (sellFs) sellFs.hidden = false;

    const qsBtn = document.getElementById('btn-quick-sale');
    if (qsBtn) qsBtn.hidden = !featureFlags.quickSaleEnabled;

    const softioCard = document.getElementById('softio-scale-card');
    if (softioCard) softioCard.hidden = !isSoftioSuperAdmin();

    const softioQsCard = document.getElementById('softio-quicksale-card');
    if (softioQsCard) softioQsCard.hidden = !isSoftioSuperAdmin();

    const toggle = document.getElementById('feature-scale-toggle');
    if (toggle) toggle.checked = featureFlags.scaleEnabled;

    const qsToggle = document.getElementById('feature-quicksale-toggle');
    if (qsToggle) qsToggle.checked = featureFlags.quickSaleEnabled;

    const status = document.getElementById('feature-scale-status');
    if (status) {
        status.textContent = featureFlags.scaleEnabled
            ? tr('feature_scale_on')
            : tr('feature_scale_off');
    }

    const qsStatus = document.getElementById('feature-quicksale-status');
    if (qsStatus) {
        qsStatus.textContent = featureFlags.quickSaleEnabled
            ? tr('feature_quicksale_on')
            : tr('feature_quicksale_off');
    }

    if (featureFlags.scaleEnabled) {
        try { scaleManager?.init?.(); } catch { /* ignore */ }
    }
}

async function loadFeatureFlags() {
    try {
        const f = await api('/api/features');
        applyFeatureFlags(f);
        if (currentUser) {
            currentUser.features = f;
            sessionStorage.setItem('generic_user', JSON.stringify(currentUser));
        }
    } catch {
        applyFeatureFlags(currentUser?.features || { scaleEnabled: false, quickSaleEnabled: false });
    }
}

function applyPrintSettings(s) {
    if (!s) return;
    const margin = (v, fb) => (Number(v) >= 0 ? Number(v) : fb);
    printSettings = {
        labelWidthMm: Number(s.labelWidthMm) || 60,
        labelHeightMm: Number(s.labelHeightMm) || 36,
        labelGapMm: margin(s.labelGapMm, 5),
        labelMarginMm: margin(s.labelMarginMm, 2),
        labelMarginTopMm: margin(s.labelMarginTopMm, margin(s.labelMarginMm, 2)),
        labelMarginRightMm: margin(s.labelMarginRightMm, margin(s.labelMarginMm, 2)),
        labelMarginBottomMm: margin(s.labelMarginBottomMm, margin(s.labelMarginMm, 2)),
        labelMarginLeftMm: margin(s.labelMarginLeftMm, margin(s.labelMarginMm, 2)),
        labelColumns: Number(s.labelColumns) >= 0 ? Number(s.labelColumns) : 0,
        labelPaperMode: (s.labelPaperMode === 'roll') ? 'roll' : 'sheet',
        labelPageWidthMm: Number(s.labelPageWidthMm) || 210,
        labelPageHeightMm: Number(s.labelPageHeightMm) || 297,
        receiptWidthMm: Number(s.receiptWidthMm) || 80,
        receiptHeightMm: margin(s.receiptHeightMm, 0),
        receiptMarginMm: margin(s.receiptMarginMm, 2.5),
        receiptMarginTopMm: margin(s.receiptMarginTopMm, margin(s.receiptMarginMm, 2.5)),
        receiptMarginRightMm: margin(s.receiptMarginRightMm, margin(s.receiptMarginMm, 2.5)),
        receiptMarginBottomMm: margin(s.receiptMarginBottomMm, margin(s.receiptMarginMm, 2.5)),
        receiptMarginLeftMm: margin(s.receiptMarginLeftMm, margin(s.receiptMarginMm, 2.5)),
        labelPrinter: s.labelPrinter || '',
        receiptPrinter: s.receiptPrinter || '',
        printerProfiles: s.printerProfiles && typeof s.printerProfiles === 'object' ? s.printerProfiles : {}
    };
    applyPrintCssVars();
}

function applyPrintCssVars() {
    const root = document.documentElement;
    root.style.setProperty('--label-w', `${printSettings.labelWidthMm}mm`);
    root.style.setProperty('--label-h', `${printSettings.labelHeightMm}mm`);
    root.style.setProperty('--label-gap', `${printSettings.labelGapMm}mm`);
    root.style.setProperty('--receipt-w', `${printSettings.receiptWidthMm}mm`);
}

function matchLabelPreset(w, h) {
    const key = Object.keys(LABEL_PRESETS).find(k =>
        LABEL_PRESETS[k].w === Number(w) && LABEL_PRESETS[k].h === Number(h));
    return key || 'custom';
}

function matchReceiptPreset(w) {
    const n = Number(w);
    if (n === 58 || n === 80 || n === 112) return String(n);
    return 'custom';
}

function resolvePrinterProfile(printerName, jobType) {
    const key = (printerName || '').trim();
    const entry = key && printSettings.printerProfiles
        ? printSettings.printerProfiles[key]
            || printSettings.printerProfiles[Object.keys(printSettings.printerProfiles).find(
                k => k.toLowerCase() === key.toLowerCase())]
        : null;
    if (jobType === 'receipt') {
        if (entry?.receipt) {
            return {
                widthMm: Number(entry.receipt.widthMm) || printSettings.receiptWidthMm,
                heightMm: Number(entry.receipt.heightMm) >= 0 ? Number(entry.receipt.heightMm) : printSettings.receiptHeightMm,
                marginMm: Number(entry.receipt.marginMm) >= 0 ? Number(entry.receipt.marginMm) : printSettings.receiptMarginMm
            };
        }
        return {
            widthMm: printSettings.receiptWidthMm,
            heightMm: printSettings.receiptHeightMm,
            marginMm: printSettings.receiptMarginMm
        };
    }
    if (entry?.label) {
        return {
            widthMm: Number(entry.label.widthMm) || printSettings.labelWidthMm,
            heightMm: Number(entry.label.heightMm) || printSettings.labelHeightMm,
            gapMm: Number(entry.label.gapMm) >= 0 ? Number(entry.label.gapMm) : printSettings.labelGapMm,
            marginMm: Number(entry.label.marginMm) >= 0 ? Number(entry.label.marginMm) : printSettings.labelMarginMm,
            paperMode: entry.label.paperMode === 'roll' ? 'roll' : 'sheet',
            pageWidthMm: Number(entry.label.pageWidthMm) || printSettings.labelPageWidthMm,
            pageHeightMm: Number(entry.label.pageHeightMm) || printSettings.labelPageHeightMm
        };
    }
    return {
        widthMm: printSettings.labelWidthMm,
        heightMm: printSettings.labelHeightMm,
        gapMm: printSettings.labelGapMm,
        marginMm: printSettings.labelMarginMm,
        paperMode: printSettings.labelPaperMode,
        pageWidthMm: printSettings.labelPageWidthMm,
        pageHeightMm: printSettings.labelPageHeightMm
    };
}

function toggleSheetPageRows() {
    const mode = document.getElementById('bpd-paper-mode')?.value
        || document.getElementById('ps-label-mode')?.value
        || 'sheet';
    const show = mode !== 'roll';
    const bpd = document.getElementById('bpd-sheet-page-row');
    const ps = document.getElementById('ps-sheet-page-row');
    if (bpd) bpd.hidden = document.getElementById('bpd-paper-mode')?.value === 'roll';
    if (ps) ps.hidden = document.getElementById('ps-label-mode')?.value === 'roll';
}

function fillPrintSettingsForm() {
    const setVal = (id, v) => { const el = document.getElementById(id); if (el) el.value = v; };
    setVal('ps-label-w', printSettings.labelWidthMm);
    setVal('ps-label-h', printSettings.labelHeightMm);
    setVal('ps-label-gap', printSettings.labelGapMm);
    setVal('ps-label-mt', printSettings.labelMarginTopMm);
    setVal('ps-label-mr', printSettings.labelMarginRightMm);
    setVal('ps-label-mb', printSettings.labelMarginBottomMm);
    setVal('ps-label-ml', printSettings.labelMarginLeftMm);
    setVal('ps-label-columns', printSettings.labelColumns ?? 0);
    setVal('ps-label-mode', printSettings.labelPaperMode);
    setVal('ps-page-w', printSettings.labelPageWidthMm);
    setVal('ps-page-h', printSettings.labelPageHeightMm);
    setVal('ps-label-preset', matchLabelPreset(printSettings.labelWidthMm, printSettings.labelHeightMm));
    setVal('ps-receipt-w', printSettings.receiptWidthMm);
    setVal('ps-receipt-h', printSettings.receiptHeightMm);
    setVal('ps-receipt-mt', printSettings.receiptMarginTopMm);
    setVal('ps-receipt-mr', printSettings.receiptMarginRightMm);
    setVal('ps-receipt-mb', printSettings.receiptMarginBottomMm);
    setVal('ps-receipt-ml', printSettings.receiptMarginLeftMm);
    setVal('ps-receipt-preset', matchReceiptPreset(printSettings.receiptWidthMm));
    setVal('ps-label-printer', printSettings.labelPrinter || '');
    setVal('ps-receipt-printer', printSettings.receiptPrinter || '');
    renderPrintProfilesList();
    toggleSheetPageRows();
}

function renderPrintProfilesList() {
    const box = document.getElementById('ps-profiles-list');
    if (!box) return;
    const profiles = printSettings.printerProfiles || {};
    const keys = Object.keys(profiles);
    if (!keys.length) {
        box.innerHTML = `<div class="empty-state" style="padding:0.75rem 0;">${tr('print_profiles_empty')}</div>`;
        return;
    }
    box.innerHTML = keys.map(name => {
        const p = profiles[name] || {};
        const label = p.label;
        const receipt = p.receipt;
        const parts = [];
        if (label?.widthMm) parts.push(`${label.widthMm}×${label.heightMm}mm label`);
        if (receipt?.widthMm) parts.push(`${receipt.widthMm}mm receipt`);
        return `<div class="ps-profile-row">
            <div><strong>${escapeHtml(name)}</strong><span>${escapeHtml(parts.join(' · ') || '—')}</span></div>
            <button type="button" class="btn-icon btn-icon-delete ps-profile-del" data-printer="${encodeURIComponent(name)}" title="${escapeHtml(tr('delete'))}">
                <span class="material-symbols-rounded">delete</span>
            </button>
        </div>`;
    }).join('');
    box.querySelectorAll('.ps-profile-del').forEach(btn => {
        btn.onclick = async () => {
            const name = decodeURIComponent(btn.dataset.printer || '');
            if (!name) return;
            try {
                await api('/api/print-settings/printer-profile?printerName=' + encodeURIComponent(name), { method: 'DELETE' });
                await loadPrintSettings();
                toast(tr('saved_ok'), 'success');
            } catch (e) { toast(e.message, 'error'); }
        };
    });
}

function fillBarcodePrintSizeControls(printerName) {
    const p = resolvePrinterProfile(printerName || document.getElementById('bpd-printer')?.value, 'label');
    const setVal = (id, v) => { const el = document.getElementById(id); if (el) el.value = v; };
    setVal('bpd-label-w', p.widthMm);
    setVal('bpd-label-h', p.heightMm);
    setVal('bpd-label-margin', p.marginMm);
    setVal('bpd-paper-mode', p.paperMode);
    setVal('bpd-page-w', p.pageWidthMm);
    setVal('bpd-page-h', p.pageHeightMm);
    setVal('bpd-label-preset', matchLabelPreset(p.widthMm, p.heightMm));
    toggleSheetPageRows();
}

function fillPosPrintSizeControls(printerName) {
    const p = resolvePrinterProfile(printerName || document.getElementById('ppd-printer')?.value, 'receipt');
    const setVal = (id, v) => { const el = document.getElementById(id); if (el) el.value = v; };
    setVal('ppd-receipt-w', p.widthMm);
    setVal('ppd-receipt-h', p.heightMm);
    setVal('ppd-receipt-margin', p.marginMm);
    setVal('ppd-receipt-preset', matchReceiptPreset(p.widthMm));
}

function readNum(id, fallback, min, max) {
    const n = parseFloat(document.getElementById(id)?.value);
    if (!Number.isFinite(n)) return fallback;
    return Math.min(max, Math.max(min, n));
}

function getLiveLabelSize() {
    return {
        labelWidthMm: readNum('bpd-label-w', printSettings.labelWidthMm, 20, 210),
        labelHeightMm: readNum('bpd-label-h', printSettings.labelHeightMm, 10, 297),
        labelGapMm: printSettings.labelGapMm,
        marginMm: readNum('bpd-label-margin', printSettings.labelMarginMm, 0, 20),
        paperMode: document.getElementById('bpd-paper-mode')?.value === 'roll' ? 'roll' : 'sheet',
        pageWidthMm: readNum('bpd-page-w', printSettings.labelPageWidthMm, 40, 330),
        pageHeightMm: readNum('bpd-page-h', printSettings.labelPageHeightMm, 40, 500)
    };
}

function getLiveReceiptSize() {
    return {
        paperWidthMm: readNum('ppd-receipt-w', printSettings.receiptWidthMm, 40, 120),
        paperHeightMm: readNum('ppd-receipt-h', printSettings.receiptHeightMm, 0, 500),
        marginMm: readNum('ppd-receipt-margin', printSettings.receiptMarginMm, 0, 15)
    };
}

async function loadPrintSettings() {
    try {
        const s = await api('/api/print-settings');
        applyPrintSettings(s);
        fillPrintSettingsForm();
    } catch {
        applyPrintSettings(printSettings);
        fillPrintSettingsForm();
    }
}

async function savePrintSettingsFromForm() {
    const body = {
        labelWidthMm: readNum('ps-label-w', 60, 20, 210),
        labelHeightMm: readNum('ps-label-h', 36, 10, 297),
        labelGapMm: readNum('ps-label-gap', 5, 0, 20),
        labelMarginTopMm: readNum('ps-label-mt', 2, 0, 30),
        labelMarginRightMm: readNum('ps-label-mr', 2, 0, 30),
        labelMarginBottomMm: readNum('ps-label-mb', 2, 0, 30),
        labelMarginLeftMm: readNum('ps-label-ml', 2, 0, 30),
        labelColumns: parseInt(document.getElementById('ps-label-columns')?.value, 10) || 0,
        labelPaperMode: document.getElementById('ps-label-mode')?.value === 'roll' ? 'roll' : 'sheet',
        labelPageWidthMm: readNum('ps-page-w', 210, 40, 330),
        labelPageHeightMm: readNum('ps-page-h', 297, 40, 500),
        receiptWidthMm: readNum('ps-receipt-w', 80, 40, 120),
        receiptHeightMm: readNum('ps-receipt-h', 0, 0, 500),
        receiptMarginTopMm: readNum('ps-receipt-mt', 2.5, 0, 20),
        receiptMarginRightMm: readNum('ps-receipt-mr', 2.5, 0, 20),
        receiptMarginBottomMm: readNum('ps-receipt-mb', 2.5, 0, 20),
        receiptMarginLeftMm: readNum('ps-receipt-ml', 2.5, 0, 20),
        labelPrinter: document.getElementById('ps-label-printer')?.value || '',
        receiptPrinter: document.getElementById('ps-receipt-printer')?.value || ''
    };
    try {
        const res = await api('/api/print-settings', {
            method: 'PUT',
            body: JSON.stringify(body)
        });
        applyPrintSettings(res);
        fillPrintSettingsForm();
        const status = document.getElementById('print-settings-status');
        if (status) status.textContent = tr('print_settings_saved');
        toast(tr('print_settings_saved'), 'success');
    } catch (err) {
        toast(err.message || tr('print_settings_fail'), 'error');
    }
}

function wirePrintSettingsUi() {
    const labelPreset = document.getElementById('ps-label-preset');
    labelPreset?.addEventListener('change', () => {
        const p = LABEL_PRESETS[labelPreset.value];
        if (!p) return;
        const w = document.getElementById('ps-label-w');
        const h = document.getElementById('ps-label-h');
        if (w) w.value = p.w;
        if (h) h.value = p.h;
    });
    ['ps-label-w', 'ps-label-h'].forEach(id => {
        document.getElementById(id)?.addEventListener('input', () => {
            if (labelPreset) {
                labelPreset.value = matchLabelPreset(
                    document.getElementById('ps-label-w')?.value,
                    document.getElementById('ps-label-h')?.value
                );
            }
        });
    });
    document.getElementById('ps-label-mode')?.addEventListener('change', toggleSheetPageRows);
    const receiptPreset = document.getElementById('ps-receipt-preset');
    receiptPreset?.addEventListener('change', () => {
        if (receiptPreset.value === 'custom') return;
        const w = document.getElementById('ps-receipt-w');
        if (w) w.value = receiptPreset.value;
    });
    document.getElementById('ps-receipt-w')?.addEventListener('input', () => {
        if (receiptPreset) receiptPreset.value = matchReceiptPreset(document.getElementById('ps-receipt-w')?.value);
    });
    document.getElementById('btn-save-print-settings')?.addEventListener('click', () => {
        savePrintSettingsFromForm();
    });
    document.getElementById('btn-clear-print-profiles')?.addEventListener('click', async () => {
        if (!await confirmDialog(tr('print_profiles_clear'), { danger: true, confirmText: tr('clear') })) return;
        try {
            await api('/api/print-settings/printer-profile?all=1', { method: 'DELETE' });
            await loadPrintSettings();
            toast(tr('saved_ok'), 'success');
        } catch (e) { toast(e.message, 'error'); }
    });

    const bpdPreset = document.getElementById('bpd-label-preset');
    bpdPreset?.addEventListener('change', () => {
        const p = LABEL_PRESETS[bpdPreset.value];
        if (!p) return;
        const w = document.getElementById('bpd-label-w');
        const h = document.getElementById('bpd-label-h');
        if (w) w.value = p.w;
        if (h) h.value = p.h;
        applyBarcodePrintLayoutPreview();
    });
    ['bpd-label-w', 'bpd-label-h', 'bpd-label-margin', 'bpd-paper-mode', 'bpd-page-w', 'bpd-page-h'].forEach(id => {
        document.getElementById(id)?.addEventListener('change', () => {
            if (id === 'bpd-paper-mode') toggleSheetPageRows();
            if ((id === 'bpd-label-w' || id === 'bpd-label-h') && bpdPreset) {
                bpdPreset.value = matchLabelPreset(
                    document.getElementById('bpd-label-w')?.value,
                    document.getElementById('bpd-label-h')?.value
                );
            }
            applyBarcodePrintLayoutPreview();
        });
        document.getElementById(id)?.addEventListener('input', () => applyBarcodePrintLayoutPreview());
    });

    document.getElementById('bpd-printer')?.addEventListener('change', () => {
        fillBarcodePrintSizeControls(document.getElementById('bpd-printer')?.value);
        applyBarcodePrintLayoutPreview();
    });

    const ppdPreset = document.getElementById('ppd-receipt-preset');
    ppdPreset?.addEventListener('change', () => {
        if (ppdPreset.value === 'custom') return;
        const w = document.getElementById('ppd-receipt-w');
        if (w) w.value = ppdPreset.value;
        applyPosPrintLayoutPreview();
    });
    ['ppd-receipt-w', 'ppd-receipt-h', 'ppd-receipt-margin'].forEach(id => {
        document.getElementById(id)?.addEventListener('input', () => {
            if (id === 'ppd-receipt-w' && ppdPreset)
                ppdPreset.value = matchReceiptPreset(document.getElementById('ppd-receipt-w')?.value);
            applyPosPrintLayoutPreview();
        });
    });
    document.getElementById('ppd-printer')?.addEventListener('change', () => {
        fillPosPrintSizeControls(document.getElementById('ppd-printer')?.value);
        applyPosPrintLayoutPreview();
    });
}

function formatPosPrice(p) {
    const base = money(p?.price || 0);
    return isSellByWeight(p) ? `${base}${tr('per_kg')}` : base;
}

function formatPosStock(p) {
    if (p.isService || p.itemType === 'Service') return tr('type_service');
    if (!isPosProductAvailable(p)) return tr('out_of_stock');
    if (isSellByWeight(p)) {
        const kg = (Number(p.stock) || 0) / 1000;
        return `${kg.toFixed(3)} ${tr('unit_kg')}`;
    }
    return `${p.stock} ${tr('col_stock').toLowerCase()}`;
}

function makeCartLine(p, qty = 1, price = null, opts = {}) {
    const tiers = productPriceTiers(p);
    const unit = price != null ? Number(price) : (tiers[0] || 0);
    const skipStock = !!(opts.skipStock || opts.allowZeroStock || opts.custom || p?.isStockTracked === false
        || p?.isService || p?.itemType === 'Service');
    return {
        id: p?.id || 0,
        name: opts.name || p?.name || '',
        price: unit,
        qty,
        max: skipStock ? 9999 : ((p.isService || p.itemType === 'Service' || p.isStockTracked === false) ? 9999 : (Number(p.stock) || 0)),
        prices: tiers.length ? tiers : [unit],
        lineKey: opts.lineKey || null,
        weighted: !!(opts.lineKey || opts.weighted || opts.weightKg),
        weightKg: opts.weightKg != null ? Number(opts.weightKg) : null,
        stockQty: opts.stockQty != null ? Number(opts.stockQty) : null,
        sellByWeight: isSellByWeight(p),
        skipStock,
        custom: !!opts.custom
    };
}

function addToCart(id, qty = 1, opts = {}) {
    const p = products.find(x => x.id === id); if (!p) return false;
    lastTappedProductId = id;
    const skipStock = !!(opts.skipStock || opts.allowZeroStock);
    if (!isPosProductAvailable(p) && !skipStock) {
        return false;
    }
    // Weighted / price-embedded scale lines stay as separate cart rows.
    if (opts.lineKey || opts.weighted) {
        if (isSellByWeight(p) && opts.stockQty > 0 && p.isStockTracked !== false && !skipStock) {
            const inCartStock = cart
                .filter(x => x.id === p.id && !x.skipStock)
                .reduce((sum, x) => sum + (x.stockQty != null ? Number(x.stockQty) : Math.round((Number(x.weightKg) || Number(x.qty) || 0) * 1000)), 0);
            if (inCartStock + opts.stockQty > (Number(p.stock) || 0)) {
                if (!opts.silentStockError) {
                    const reqKg = (opts.stockQty / 1000).toFixed(3);
                    const remKg = (Math.max(0, (Number(p.stock) || 0) - inCartStock) / 1000).toFixed(3);
                    showOutOfStockWeightPopup(p.name, reqKg, remKg);
                }
                return false;
            }
        }
        const row = makeCartLine(p, qty, opts.price != null ? opts.price : null, { ...opts, skipStock });
        cart.push(row);
        renderCart();
        return true;
    }
    if (isSellByWeight(p) && !opts.weighted) {
        toast(tr('weigh_first'), 'info');
        return false;
    }
    const line = cart.find(x => x.id === id && !x.weighted && !!x.skipStock === !!skipStock);
    if (line) {
        if (line.qty + qty > line.max) {
            if (!skipStock) showOutOfStockPopup(p.name);
            return false;
        }
        line.qty += qty;
    } else {
        const row = makeCartLine(p, qty, opts.price != null ? opts.price : null, { ...opts, skipStock });
        if (row.qty > row.max) {
            if (!skipStock) showOutOfStockPopup(p.name);
            return false;
        }
        cart.push(row);
    }
    renderCart();
    return true;
}

function addCustomCartLine(name, price, qty = 1) {
    const cleanName = String(name || '').trim();
    const unit = Number(price);
    const q = Math.max(1, Math.min(9999, parseInt(qty, 10) || 1));
    if (!cleanName) {
        toast(tr('qs_need_name'), 'error');
        return false;
    }
    if (!(unit > 0)) {
        toast(tr('qs_need_price'), 'error');
        return false;
    }
    cart.push({
        id: 0,
        name: cleanName,
        price: unit,
        qty: q,
        max: 9999,
        prices: [unit],
        lineKey: `custom-${Date.now()}`,
        weighted: false,
        weightKg: null,
        stockQty: null,
        sellByWeight: false,
        skipStock: true,
        custom: true
    });
    renderCart();
    return true;
}

const exportCsv = (rows, filename) => {
    if (!rows.length) return toast(tr('empty_list'), 'error');
    const keys = Object.keys(rows[0]);
    const lines = [keys.join(',')].concat(rows.map(r => keys.map(k => {
        const v = String(r[k] ?? '').replace(/"/g, '""');
        return `"${v}"`;
    }).join(',')));
    const blob = new Blob([lines.join('\n')], { type: 'text/csv;charset=utf-8;' });
    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = filename;
    a.click();
    URL.revokeObjectURL(a.href);
};

async function downloadServerExport(path, fallbackName, onFallback) {
    try {
        const res = await fetch(API + path, { credentials: 'same-origin' });
        if (!res.ok) {
            if (res.status === 404 && typeof onFallback === 'function') {
                onFallback();
                toast(tr('export_csv_fallback') || 'Exported as CSV — restart app for Excel auto-fit columns', 'success');
                return;
            }
            let msg = res.statusText;
            try { const j = await res.json(); msg = j.error || j.title || msg; } catch { /* ignore */ }
            throw new Error(msg || 'Export failed');
        }
        const blob = await res.blob();
        const cd = res.headers.get('content-disposition') || '';
        const match = /filename\*?=(?:UTF-8''|")?([^";]+)/i.exec(cd);
        const filename = match ? decodeURIComponent(match[1].replace(/"/g, '')) : fallbackName;
        const a = document.createElement('a');
        a.href = URL.createObjectURL(blob);
        a.download = filename;
        a.click();
        URL.revokeObjectURL(a.href);
    } catch (e) { toast(e.message, 'error'); }
}

function toastImportResult(res) {
    const ok = (res?.imported || 0) + (res?.updated || 0);
    if (!ok) return toast(tr('empty_list'), 'error');
    let msg = tr('import_ok');
    if (res.imported) msg += ` (+${res.imported})`;
    if (res.updated) msg += ` (~${res.updated})`;
    if (res.skipped) msg += `, skip ${res.skipped}`;
    toast(msg, 'success');
}

function productExportRow(p) {
    const imagePath = p.imagePath || (p.image ? String(p.image).replace(/^\//, '') : '');
    return {
        part_number: p.sku || '',
        part_name: p.name || '',
        description: p.description || '',
        category_name: p.category || 'General',
        supplier_id: p.supplierId ?? '',
        supplier_name: p.supplierName || '',
        quantity_in_stock: p.stock ?? 0,
        selling_price: p.price ?? 0,
        purchase_price: p.cost ?? 0,
        minimum_stock_level: p.minStock ?? 0,
        reorder_quantity: p.reorderQuantity ?? 0,
        location: p.location || '',
        shelf: p.shelf || '',
        barcode: p.barcode || '',
        unit_of_measure: p.uom || '',
        batch_number: p.batch || '',
        expiry_date: p.expiry || '',
        item_type: p.itemType || 'Product',
        is_sales_item: p.isSalesItem !== false ? 1 : 0,
        is_purchase_item: p.isPurchaseItem ? 1 : 0,
        is_inactive: p.isInactive ? 1 : 0,
        tax_rate: p.taxRate ?? 0,
        is_stock_tracked: p.isStockTracked !== false ? 1 : 0,
        sell_by_weight: p.sellByWeight ? 1 : 0,
        price2: p.price2 ?? 0,
        price3: p.price3 ?? 0,
        price4: p.price4 ?? 0,
        part_image: imagePath,
        status: p.status || 'Active'
    };
}

function customerExportRow(c) {
    return {
        CustomerName: c.name || '',
        Phone: c.phone || '',
        Email: c.email || '',
        Address: c.address || '',
        CustomerType: c.type || 'Regular',
        Balance: c.balance ?? 0,
        CreditLimit: c.creditLimit ?? 1000,
        PaymentDueDate: c.dueDate ? String(c.dueDate).slice(0, 10) : '',
        ReminderDays: c.reminderDays ?? 0
    };
}

function supplierExportRow(s) {
    return {
        SupplierName: s.name || '',
        ContactPerson: s.contact || '',
        Email: s.email || '',
        Phone: s.phone || '',
        Address: s.address || '',
        Type: s.type || 'Regular',
        City: '',
        PostalCode: '',
        Website: '',
        Notes: '',
        Balance: s.balance ?? 0,
        PaymentDueDate: s.dueDate ? String(s.dueDate).slice(0, 10) : '',
        ReminderDays: s.reminderDays ?? 0
    };
}

function historyExportRow(r) {
    return {
        date: csvCell(r, 'date', 'order_date', 'timestamp', 'orderdate'),
        action: csvCell(r, 'action', 'action_type', 'type'),
        item: csvCell(r, 'item', 'part_name', 'name', 'sku'),
        customer: csvCell(r, 'customer', 'customer_name', 'customername'),
        details: csvCell(r, 'details', 'description', 'desc'),
        user: csvCell(r, 'user', 'username'),
        status: csvCell(r, 'status'),
        payment: csvCell(r, 'payment', 'payment_status', 'paymentstatus'),
        total: parseFloat(csvCell(r, 'total', 'total_amount', 'amount') || '0') || 0
    };
}




function parseCsvLine(line) {
    const cols = [];
    let cur = '', inQ = false;
    for (let i = 0; i < line.length; i++) {
        const ch = line[i];
        if (inQ) {
            if (ch === '"' && line[i + 1] === '"') { cur += '"'; i++; }
            else if (ch === '"') inQ = false;
            else cur += ch;
        } else if (ch === '"') inQ = true;
        else if (ch === ',') { cols.push(cur); cur = ''; }
        else cur += ch;
    }
    cols.push(cur);
    return cols;
}

async function parseCsvFile(file) {
    const text = await file.text();
    const lines = text.split(/\r?\n/).filter(l => l.trim());
    if (lines.length < 2) return { headers: [], rows: [] };
    const headers = parseCsvLine(lines[0]).map(h => h.trim().toLowerCase());
    const rows = lines.slice(1).map(line => {
        const cols = parseCsvLine(line);
        const obj = {};
        headers.forEach((h, i) => { obj[h] = (cols[i] ?? '').trim(); });
        return obj;
    });
    return { headers, rows };
}

function csvCell(row, ...keys) {
    for (const k of keys) {
        const key = String(k || '').toLowerCase();
        if (row[key] != null && String(row[key]).trim() !== '') return String(row[key]).trim();
    }
    return '';
}

const actionBtns = (editAttr, deleteAttr, extra = '') => `<div class="table-actions">
    ${extra}
    <button type="button" class="btn-icon btn-icon-edit" title="${tr('edit')}" ${editAttr}><span class="material-symbols-rounded">edit</span></button>
    <button type="button" class="btn-icon btn-icon-delete" title="${tr('delete')}" ${deleteAttr}><span class="material-symbols-rounded">delete</span></button>
</div>`;

const tr = (k) => (T[lang] && T[lang][k]) || T.en[k] || String(k || '').replace(/_/g, ' ');

function formatRole(role) {
    const r = String(role || '').trim().toLowerCase();
    if (r === 'admin') return tr('role_admin');
    if (r === 'staff') return tr('role_staff');
    if (r === 'accountant') return tr('role_accountant');
    return role || '—';
}
const money = (n) => '$' + (Number(n) || 0).toFixed(2);
const posCurrencyMeta = () => {
    const sel = document.getElementById('pos-currency');
    const code = (sel?.value || 'USD').toString().trim().toUpperCase() || 'USD';
    const cur = (currencies || []).find(c => String(c.code || c.Code || '').toUpperCase() === code);
    let rate = Number(cur?.rate ?? cur?.Rate);
    if (!(rate > 0)) rate = code === 'USD' ? 1 : 1;
    const symbol = (cur?.symbol || cur?.Symbol || (code === 'USD' ? '$' : code === 'LBP' ? 'ل.ل.' : code + ' ')).toString();
    return { code, rate, symbol };
};
/** Format a USD-base amount in the active POS currency (with commas; LBP has no decimals). */
function formatPosAmount(usdAmount) {
    const { code, rate } = posCurrencyMeta();
    const converted = (Number(usdAmount) || 0) * rate;
    if (code === 'LBP') return Math.round(converted).toLocaleString('en-US');
    return converted.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}
const posMoney = (n) => {
    const { code, symbol } = posCurrencyMeta();
    const amt = formatPosAmount(n);
    return code === 'LBP' ? `${symbol} ${amt}` : `${symbol}${amt}`;
};
function parsePosDisplayAmount(str) {
    const cleaned = String(str ?? '').replace(/[^\d.-]/g, '');
    if (!cleaned || cleaned === '-' || cleaned === '.') return NaN;
    const v = parseFloat(cleaned);
    return Number.isFinite(v) ? v : NaN;
}
function posDisplayToBase(displayAmt) {
    const { rate } = posCurrencyMeta();
    const r = rate > 0 ? rate : 1;
    return (Number(displayAmt) || 0) / r;
}

let posTotalManual = null; // always stored in USD base

function getPosTotals() {
    const sub = cart.reduce((s, x) => s + x.price * x.qty, 0);
    const vatOn = document.getElementById('pos-vat')?.checked;
    const shipOn = document.getElementById('pos-ship')?.checked;
    const discOn = document.getElementById('pos-disc')?.checked;
    const vat = vatOn ? sub * POS_VAT_RATE : 0;
    const ship = shipOn ? (parseFloat(document.getElementById('pos-ship-amt')?.value) || 0) : 0;
    const discPct = discOn ? Math.min(100, Math.max(0, parseFloat(document.getElementById('pos-disc-amt')?.value) || 0)) : 0;
    const disc = +(sub * discPct / 100).toFixed(2);
    const calc = Math.max(0, sub + vat + ship - disc);
    const totalInp = document.getElementById('pos-total-amt');
    let manual = posTotalManual;
    if (manual == null && totalInp && document.activeElement === totalInp) {
        const typed = parsePosDisplayAmount(totalInp.value);
        if (Number.isFinite(typed)) manual = Math.max(0, posDisplayToBase(typed));
    }
    const total = manual != null ? Math.max(0, manual) : calc;
    return { sub, vat, ship, disc, calc, total, vatOn, shipOn, discOn, manual: manual != null };
}
const escapeHtml = (s) => String(s ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
const formatDate = (d) => { try { return new Date(d).toLocaleString(lang === 'ar' ? 'ar' : 'en'); } catch { return d; } };

function applyI18n() {
    document.documentElement.lang = lang;
    document.body.classList.toggle('rtl', lang === 'ar');
    document.querySelectorAll('[data-i18n]').forEach(el => { el.textContent = tr(el.getAttribute('data-i18n')); });
    document.querySelectorAll('[data-i18n-ph]').forEach(el => { el.placeholder = tr(el.getAttribute('data-i18n-ph')); });
    document.querySelectorAll('[data-i18n-title]').forEach(el => {
        const t = tr(el.getAttribute('data-i18n-title'));
        el.title = t;
        if (el.hasAttribute('aria-label') || el.getAttribute('data-i18n-aria') === '1') el.setAttribute('aria-label', t);
    });
    const langLabel = document.getElementById('lang-label');
    if (langLabel) langLabel.textContent = lang === 'en' ? 'العربية' : 'English';
    updateSettingsLangLabel();
    try { scaleManager?.render?.(); } catch { /* scale not ready yet */ }
}

function openModal(id) {
    const modal = document.getElementById(id);
    if (!modal) return;
    modal.classList.add('active');
}

function closeModal(id) {
    if (id === 'license-modal' && document.getElementById('license-modal')?.classList.contains('license-wall-blocking')) {
        const lic = window._license;
        if (lic && (lic.isValid === false || (lic.daysRemaining !== undefined && lic.daysRemaining <= 0))) {
            return;
        }
    }
    const modal = document.getElementById(id);
    if (!modal) return;
    modal.classList.remove('active');
}

function toast(msg, type = '') {
    const el = document.getElementById('toast');
    el.textContent = msg; el.className = 'toast show ' + type;
    setTimeout(() => el.classList.remove('show'), 2800);
}

async function api(path, opts = {}) {
    const res = await fetch(API + path, { headers: { 'Content-Type': 'application/json', ...(opts.headers || {}) }, ...opts });
    if (!res.ok) {
        let err = res.statusText;
        try { const j = await res.json(); err = j.error || j.title || j.detail || err; } catch {}
        throw new Error(err);
    }
    if (res.status === 204) return null;
    return res.json();
}

function userRoles() {
    if (!currentUser) return [];
    const roles = [];
    if (currentUser.isAdmin) roles.push('admin');
    if (currentUser.isStaff) roles.push('staff');
    if (currentUser.isAccountant) roles.push('accountant');
    if (!roles.length) {
        const r = (currentUser.role || '').toLowerCase();
        if (r.includes('admin')) roles.push('admin', 'staff', 'accountant');
        else if (r.includes('account')) roles.push('accountant');
        else roles.push('staff');
    }
    return roles;
}

function can(roleList) {
    if (!roleList) return true;
    const allowed = roleList.split(',').map(s => s.trim());
    return userRoles().some(r => allowed.includes(r));
}

function applyRolePermissions() {
    document.querySelectorAll('[data-roles]').forEach(el => {
        el.style.display = can(el.getAttribute('data-roles')) ? '' : 'none';
    });
}

function restoreRouteFromHash() {
    const target = (location.hash || '').replace(/^#\/?/, '');
    const item = target
        ? document.querySelector(`.nav-menu .nav-item[data-target="${target}"]`)
        : null;
    if (item && item.style.display !== 'none' && document.getElementById(target)) {
        navigateTo(target, false);
        return;
    }
    const first = [...document.querySelectorAll('.nav-menu .nav-item[data-target]')]
        .find(n => n.style.display !== 'none');
    if (first) navigateTo(first.getAttribute('data-target'), true);
}

function showApp() {
    const login = document.getElementById('login-screen');
    const app = document.getElementById('app-container');
    if (login) {
        login.classList.add('hidden');
        login.style.display = 'none';
    }
    if (app) {
        app.classList.add('visible');
        app.style.display = 'flex';
    }
    const tools = document.getElementById('titlebar-tools');
    if (tools) tools.hidden = false;
    if (currentUser) {
        const name = currentUser.fullName || currentUser.username;
        const curUser = document.getElementById('current-user');
        if (curUser) curUser.textContent = name;
        const tbName = document.getElementById('tb-user-name');
        if (tbName) tbName.textContent = name;
    }
    applyRolePermissions();
    refreshNotifications();
    renderDashboardSkeleton();
    restoreRouteFromHash();
}

function hideApp() {
    const login = document.getElementById('login-screen');
    const app = document.getElementById('app-container');
    if (login) {
        login.classList.remove('hidden');
        login.style.display = '';
    }
    if (app) {
        app.classList.remove('visible');
        app.style.display = 'none';
    }
    const tools = document.getElementById('titlebar-tools');
    if (tools) tools.hidden = true;
    currentUser = null;
    sessionStorage.removeItem('generic_user');
    const userInput = document.getElementById('login-user');
    const passInput = document.getElementById('login-pass');
    if (userInput) userInput.value = '';
    if (passInput) passInput.value = '';
    const err = document.getElementById('login-error');
    if (err) {
        err.textContent = '';
        err.classList.remove('visible');
    }
    if (userInput) userInput.focus();
}

/**
 * @param {{ fromSignalR?: boolean }} [opts]
 */
async function loadData(opts = {}) {
    if (opts.fromSignalR && Date.now() < _suppressSignalRReloadUntil) {
        return _loadDataInflight;
    }
    if (_loadDataInflight) {
        _loadDataAgain = true;
        return _loadDataInflight;
    }
    _loadDataInflight = (async () => {
        try {
            do {
                _loadDataAgain = false;
                await loadDataCore();
            } while (_loadDataAgain);
        } finally {
            _loadDataInflight = null;
        }
    })();
    return _loadDataInflight;
}

function suppressSignalRReload(ms = 1500) {
    _suppressSignalRReloadUntil = Date.now() + ms;
}

function normalizeProductRow(x) {
    const row = {
        ...x,
        sellByWeight: isSellByWeight(x) || !!(x.sellByWeight || x.sell_by_weight)
    };
    row.sellByWeight = isSellByWeight(row);
    return row;
}

function productFromPayload(payload, id) {
    return normalizeProductRow({
        id,
        name: payload.name,
        description: payload.description || '',
        category: payload.category || 'General',
        price: payload.price,
        cost: payload.cost || 0,
        stock: payload.stock || 0,
        minStock: payload.minStock || 0,
        barcode: payload.barcode || '',
        sku: payload.sku || '',
        image: payload.image || '',
        location: payload.location || '',
        shelf: payload.shelf || '',
        uom: payload.uom || '',
        batch: payload.batch || '',
        expiry: payload.expiry || '',
        itemType: payload.itemType || 'Product',
        isService: (payload.itemType || 'Product') === 'Service',
        isSalesItem: payload.isSalesItem !== false,
        isPurchaseItem: !!payload.isPurchaseItem,
        isInactive: !!payload.isInactive,
        taxRate: payload.taxRate || 0,
        isStockTracked: payload.isStockTracked !== false,
        sellByWeight: !!payload.sellByWeight,
        price2: payload.price2 || 0,
        price3: payload.price3 || 0,
        price4: payload.price4 || 0,
        supplierId: payload.supplierId ?? null,
        status: payload.isInactive ? 'Inactive' : 'Active'
    });
}

function insertOptimisticProduct(payload) {
    const temp = productFromPayload(payload, -Date.now());
    products = [temp, ...products.filter(p => p.id !== temp.id)];
    if (temp.category && !categories.includes(temp.category)) {
        categories = [...categories, temp.category];
    }
    try {
        renderInventory();
        renderPOS();
        updateBadges();
    } catch (e) { console.error(e); }
}

async function loadDataCore() {
    const isAdmin = can('admin');
    const isStaffOrAcc = can('admin,staff,accountant');

    const settled = await Promise.allSettled([
        api('/api/products?includeInactive=1'),
        api('/api/categories'),
        api('/api/recent-sales'),
        api('/api/dashboard'),
        api('/api/customers'),
        api('/api/suppliers'),
        api('/api/currencies'),
        isStaffOrAcc ? api('/api/expenses') : Promise.resolve([]),
        isStaffOrAcc ? api('/api/quotations') : Promise.resolve([]),
        isStaffOrAcc ? api('/api/expense-categories') : Promise.resolve([]),
        isAdmin ? api('/api/users') : Promise.resolve([]),
        api('/api/barcode/items')
    ]);

    const val = (i) => (settled[i].status === 'fulfilled' ? settled[i].value : null);
    const p = val(0), c = val(1), s = val(2), d = val(3);
    const cust = val(4), supp = val(5), curr = val(6);
    const exp = val(7), quot = val(8), expCat = val(9);
    const usr = val(10), bc = val(11);

    if (p) products = (p || []).map(normalizeProductRow);
    if (c) categories = c || [];
    if (s) sales = s || [];
    if (d) dashboard = d || {};
    if (cust) customers = cust || [];
    if (supp) suppliers = supp || [];
    if (curr) currencies = curr || [];
    if (exp) expenses = exp || [];
    if (quot) quotations = quot || [];
    if (expCat) expenseCategories = expCat || [];
    if (usr) users = usr || [];
    if (bc) barcodeItems = bc || [];
    else barcodeItems = products.map(x => ({ id: x.id, name: x.name, sku: x.sku, price: x.price, barcode: x.barcode, stock: x.stock }));

    renderDashboard();
    const connectTask = loadConnectInfo();
    const extraTasks = [];
    if (isStaffOrAcc) {
        extraTasks.push(loadReports(), loadHistory());
    }
    extraTasks.push(loadUoms(), loadLicense(), connectTask);
    await Promise.allSettled(extraTasks);

    renderAll();
    refreshNotifications();
}

async function loadConnectInfo() {
    const link = document.getElementById('dash-connect-url');
    const qr = document.getElementById('dash-connect-qr');
    const wrap = document.querySelector('.dash-connect-qr-wrap');
    const card = document.getElementById('dash-connect-card');
    try {
        const info = await api('/api/connect');
        if (!info?.url) throw new Error('no url');
        if (link) {
            link.href = info.url;
            link.textContent = info.url;
        }
        if (qr) {
            wrap?.classList.add('is-loading');
            qr.classList.remove('is-error');
            if (!qr.getAttribute('src')) qr.src = QR_PLACEHOLDER;
            try {
                const res = await fetch(API + '/api/connect/qr?t=' + Date.now());
                if (!res.ok) throw new Error('qr ' + res.status);
                const blob = await res.blob();
                if (!blob || !blob.type.startsWith('image/')) throw new Error('not image');
                if (_connectQrObjectUrl) {
                    try { URL.revokeObjectURL(_connectQrObjectUrl); } catch { /* ignore */ }
                }
                _connectQrObjectUrl = URL.createObjectURL(blob);
                qr.src = _connectQrObjectUrl;
            } catch (qrErr) {
                console.error('connect qr', qrErr);
                qr.src = QR_PLACEHOLDER;
                qr.classList.add('is-error');
            } finally {
                wrap?.classList.remove('is-loading');
            }
        }
        card?.removeAttribute('hidden');
    } catch (e) {
        console.error('connect info', e);
        if (link) {
            link.href = '#';
            link.textContent = '—';
        }
        if (qr) {
            qr.src = QR_PLACEHOLDER;
            qr.classList.add('is-error');
        }
        wrap?.classList.remove('is-loading');
    }
}

async function loadReports() {
    const preset = document.getElementById('report-preset')?.value || 'Monthly';
    const fromEl = document.getElementById('report-from');
    const toEl = document.getElementById('report-to');
    syncReportDateInputs(preset);
    const from = fromEl?.value || '';
    const to = toEl?.value || '';
    const qs = new URLSearchParams({ preset, limit: '15' });
    if (from && to) {
        qs.set('from', from);
        qs.set('to', to);
    }
    try {
        reportSummary = await api('/api/reports/summary?' + qs.toString());
        reportTop = await api('/api/reports/top-products?' + qs.toString());
        try {
            reportExpensesBreakdown = await api('/api/reports/expenses-breakdown?' + qs.toString());
        } catch {
            reportExpensesBreakdown = { milkSuppliers: [], categories: [] };
        }
        // Keep date inputs aligned with the range the API actually used
        if (reportSummary?.fromDate && fromEl && preset !== 'Custom') {
            fromEl.value = toInputDate(reportSummary.fromDate);
        }
        if (reportSummary?.toDate && toEl && preset !== 'Custom') {
            toEl.value = toInputDate(reportSummary.toDate);
        }
    } catch { reportSummary = null; reportTop = []; reportExpensesBreakdown = { milkSuppliers: [], categories: [] }; }
}

function toInputDate(v) {
    if (!v) return '';
    const s = String(v);
    if (/^\d{4}-\d{2}-\d{2}/.test(s)) return s.slice(0, 10);
    try {
        const d = new Date(v);
        if (Number.isNaN(d.getTime())) return '';
        return formatLocalDate(d);
    } catch { return ''; }
}

function formatLocalDate(d) {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

function syncReportDateInputs(preset) {
    const fromEl = document.getElementById('report-from');
    const toEl = document.getElementById('report-to');
    if (!fromEl || !toEl) return;
    const today = new Date();
    const setRange = (from, to) => {
        fromEl.value = formatLocalDate(from);
        toEl.value = formatLocalDate(to);
    };
    if (preset === 'Custom') {
        if (!fromEl.value) fromEl.value = formatLocalDate(today);
        if (!toEl.value) toEl.value = formatLocalDate(today);
        fromEl.disabled = false;
        toEl.disabled = false;
        return;
    }
    fromEl.disabled = false;
    toEl.disabled = false;
    if (preset === 'Weekly') {
        const start = new Date(today);
        start.setDate(today.getDate() - today.getDay());
        setRange(start, today);
    } else if (preset === 'Monthly') {
        setRange(new Date(today.getFullYear(), today.getMonth(), 1), today);
    } else if (preset === 'Yearly') {
        setRange(new Date(today.getFullYear(), 0, 1), today);
    } else {
        setRange(today, today);
    }
}

async function loadHistory() {
    const kind = document.getElementById('history-kind')?.value || 'orders';
    try { window._historyRows = await api('/api/history/' + kind); }
    catch { window._historyRows = []; }
}

async function loadLicense() {
    try {
        window._license = await api('/api/license');
        checkLicenseExpirationWall();
    } catch { window._license = null; }
}

function checkLicenseExpirationWall() {
    const lic = window._license;
    if (!lic) return;
    const isExpired = lic.isValid === false || (lic.daysRemaining !== undefined && lic.daysRemaining <= 0);
    const modal = document.getElementById('license-modal');
    if (!modal) return;

    if (isExpired) {
        modal.classList.add('license-wall-blocking');
        const err = document.getElementById('license-activate-error');
        if (err) {
            err.textContent = lic.isTrial ? tr('trial_expired_msg') : tr('license_expired_msg');
            err.classList.add('visible');
        }
        openLicenseModal();
        const closeBtn = modal.querySelector('.close-btn');
        if (closeBtn) closeBtn.style.display = 'none';
        const cancelBtn = document.getElementById('btn-lic-cancel') || modal.querySelector('[data-close="license-modal"]');
        if (cancelBtn) cancelBtn.style.display = 'none';
        const startTrialBtn = document.getElementById('btn-start-trial');
        if (startTrialBtn) startTrialBtn.style.display = 'none';
        const closeAppBtn = document.getElementById('btn-lic-close-app');
        if (closeAppBtn) {
            closeAppBtn.style.display = '';
            closeAppBtn.onclick = () => postHost('close');
        }
    } else {
        modal.classList.remove('license-wall-blocking');
        const closeBtn = modal.querySelector('.close-btn');
        if (closeBtn) {
            closeBtn.style.display = '';
            closeBtn.onclick = () => closeModal('license-modal');
        }
        const cancelBtn = document.getElementById('btn-lic-cancel') || modal.querySelector('[data-close="license-modal"]');
        if (cancelBtn) cancelBtn.style.display = '';
        const closeAppBtn = document.getElementById('btn-lic-close-app');
        if (closeAppBtn) closeAppBtn.style.display = 'none';

        if (lic.isValid && lic.daysRemaining <= 3 && lic.daysRemaining > 0) {
            toast(tr('license_expiring_soon_msg').replace('{0}', lic.daysRemaining), 'warning');
        }
    }
}

function renderAll() {
    try { renderDashboard(); } catch (e) { console.error(e); }
    try { renderInventory(); } catch (e) { console.error(e); }
    try { renderPOS(); } catch (e) { console.error(e); }
    try { renderSales(); } catch (e) { console.error(e); }
    try { renderReports(); } catch (e) { console.error(e); }
    try { renderHistory(); } catch (e) { console.error(e); }
    try { renderQuotations(); } catch (e) { console.error(e); }
    try { renderCustomers(); } catch (e) { console.error(e); }
    try { renderSuppliers(); } catch (e) { console.error(e); }
    try { renderExpenses(); } catch (e) { console.error(e); }
    try { renderCurrencies(); } catch (e) { console.error(e); }
    try { renderBarcodes(); } catch (e) { console.error(e); }
    try { renderUsers(); } catch (e) { console.error(e); }
    try { renderLicense(); } catch (e) { console.error(e); }
    try { updateBadges(); } catch (e) { console.error(e); }
}

function stockBadge(p) {
    if (p.isService || p.itemType === 'Service') return `<span class="badge service">${tr('type_service')}</span>`;
    if (p.isStockTracked === false) return `<span class="badge in-stock">—</span>`;
    if (p.stock <= 0) return `<span class="badge out-of-stock">${tr('out_of_stock')}</span>`;
    if (p.stock <= (p.minStock || 0)) return `<span class="badge low-stock">${tr('low_stock')}</span>`;
    return `<span class="badge in-stock">${tr('in_stock')}</span>`;
}

function renderDashboardSkeleton() {
    const grid = document.getElementById('stats-grid');
    if (grid) {
        grid.innerHTML = `
        <div class="stat-card is-skeleton"><div class="icon"><span class="material-symbols-rounded">payments</span></div>
            <div class="label">${tr('today_sales')}</div><div class="value">0</div></div>
        <div class="stat-card is-skeleton"><div class="icon"><span class="material-symbols-rounded">inventory</span></div>
            <div class="label">${tr('inventory_value')}</div><div class="value">0</div></div>
        <div class="stat-card is-skeleton"><div class="icon"><span class="material-symbols-rounded">category</span></div>
            <div class="label">${tr('total_items')}</div><div class="value">0</div></div>
        <div class="stat-card is-skeleton"><div class="icon"><span class="material-symbols-rounded">warning</span></div>
            <div class="label">${tr('low_stock_count')}</div><div class="value">0</div></div>
        <div class="stat-card is-skeleton"><div class="icon"><span class="material-symbols-rounded">shopping_bag</span></div>
            <div class="label">${tr('orders_today')}</div><div class="value">0</div></div>`;
    }
    const loadingRow = `<tr class="dash-loading-row"><td colspan="3">${tr('loading')}</td></tr>`;
    const prodBody = document.getElementById('dash-top-products-body');
    const catBody = document.getElementById('dash-top-categories-body');
    const salesBody = document.getElementById('dash-sales-body');
    if (prodBody) prodBody.innerHTML = loadingRow;
    if (catBody) catBody.innerHTML = loadingRow;
    if (salesBody) salesBody.innerHTML = `<tr class="dash-loading-row"><td colspan="4">${tr('loading')}</td></tr>`;
    const link = document.getElementById('dash-connect-url');
    if (link && (!link.textContent || link.textContent === '—')) link.textContent = tr('loading');
}

function renderDashboard() {
    const d = dashboard || {};
    document.getElementById('stats-grid').innerHTML = `
        <div class="stat-card"><div class="icon"><span class="material-symbols-rounded">payments</span></div>
            <div class="label">${tr('today_sales')}</div><div class="value">${money(d.todaySales)}</div></div>
        <div class="stat-card"><div class="icon"><span class="material-symbols-rounded">inventory</span></div>
            <div class="label">${tr('inventory_value')}</div><div class="value">${money(d.inventoryValue)}</div></div>
        <div class="stat-card"><div class="icon"><span class="material-symbols-rounded">category</span></div>
            <div class="label">${tr('total_items')}</div><div class="value">${d.totalItems ?? 0}</div></div>
        <div class="stat-card"><div class="icon"><span class="material-symbols-rounded">warning</span></div>
            <div class="label">${tr('low_stock_count')}</div><div class="value">${d.lowStock ?? 0}</div></div>
        <div class="stat-card"><div class="icon"><span class="material-symbols-rounded">shopping_bag</span></div>
            <div class="label">${tr('orders_today')}</div><div class="value">${d.ordersToday ?? 0}</div></div>`;

    const topProducts = normalizeDashRows(d.topProducts);
    const topCategories = normalizeDashRows(d.topCategories);
    const prodBody = document.getElementById('dash-top-products-body');
    if (prodBody) {
        prodBody.innerHTML = topProducts.length
            ? topProducts.map(r => `<tr>
                <td>${escapeHtml(r.name)}</td>
                <td>${Number(r.qtySold) || 0}</td>
                <td>${money(r.totalSales)}</td>
            </tr>`).join('')
            : `<tr><td colspan="3" class="empty-state">${tr('empty_list')}</td></tr>`;
    }
    const catBody = document.getElementById('dash-top-categories-body');
    if (catBody) {
        catBody.innerHTML = topCategories.length
            ? topCategories.map(r => `<tr>
                <td>${escapeHtml(r.name)}</td>
                <td>${Number(r.qtySold) || 0}</td>
                <td>${money(r.totalSales)}</td>
            </tr>`).join('')
            : `<tr><td colspan="3" class="empty-state">${tr('empty_list')}</td></tr>`;
    }

    document.getElementById('dash-sales-body').innerHTML = (sales.slice(0, 8).map(o => `
        <tr><td>#${o.orderId}</td><td>${formatDate(o.date)}</td><td>${escapeHtml(o.customer)}</td><td>${money(o.total)}</td></tr>`).join(''))
        || `<tr><td colspan="4" class="empty-state">${tr('empty_list')}</td></tr>`;
}

function normalizeDashRows(rows) {
    if (!Array.isArray(rows)) return [];
    return rows.map(r => ({
        name: r.name || r.Name || r.part_name || r.category_name || r.product_name || '—',
        qtySold: r.qtySold ?? r.QtySold ?? r.total_sold ?? r.quantity_sold ?? 0,
        totalSales: r.totalSales ?? r.TotalSales ?? r.total_sales ?? 0
    })).filter(r => r.name && r.name !== '—');
}

function renderCategoryPills(containerId, active, onClick) {
    const el = document.getElementById(containerId);
    if (!el) return;
    el.innerHTML = ['all', ...categories].map(c => {
        const key = c === 'all' ? 'all' : c;
        return `<button type="button" class="cat-pill ${active === key ? 'active' : ''}" data-cat="${escapeHtml(key)}">${escapeHtml(c === 'all' ? tr('all') : c)}</button>`;
    }).join('');
    el.querySelectorAll('.cat-pill').forEach(btn => btn.onclick = () => onClick(btn.getAttribute('data-cat')));
}

function countPosCategory(key) {
    return products.filter(p => {
        if (p.isInactive || p.status === 'Inactive') return false;
        if (key === 'all') return true;
        return p.category === key;
    }).length;
}

function renderPosCategoryChips() {
    const el = document.getElementById('pos-cats');
    if (!el) return;
    const chips = [{ key: 'all', label: tr('all_categories'), icon: 'grid_view' }]
        .concat((categories || []).map(c => ({ key: c, label: c, icon: getCategoryIcon(c) })));
    el.innerHTML = chips.map(c => {
        const count = countPosCategory(c.key);
        const active = posCat === c.key ? ' active' : '';
        return `<button type="button" class="pos-cat-card${active}" data-cat="${escapeHtml(c.key)}" title="${escapeHtml(c.label)}">
            <span class="pos-cat-icon"><span class="material-symbols-rounded">${c.icon}</span></span>
            <span class="pos-cat-text">
                <strong title="${escapeHtml(c.label)}">${escapeHtml(c.label)}</strong>
                <small class="pos-cat-count">${count} ${tr('items_count')}</small>
            </span>
        </button>`;
    }).join('');
    el.querySelectorAll('.pos-cat-card').forEach(btn => {
        btn.onclick = () => { posCat = btn.getAttribute('data-cat'); renderPOS(); };
    });
    if (!el.dataset.wheelBound) {
        el.dataset.wheelBound = '1';
        el.addEventListener('wheel', e => {
            if (e.deltaY !== 0) {
                e.preventDefault();
                el.scrollLeft += e.deltaY;
            }
        }, { passive: false });
    }
}

function scrollPosCats(dir) {
    const el = document.getElementById('pos-cats');
    if (!el) return;
    el.scrollBy({ left: dir * 220, behavior: 'smooth' });
}

function renderPOS() {
    renderPosStats();
    renderPosCategoryChips();
    const list = filteredProducts(document.getElementById('pos-search')?.value, posCat, { forPos: true });
    const grid = document.getElementById('pos-products');
    grid.innerHTML = list.length ? list.map(p => {
        const img = p.image || p.categoryImage || '';
        const available = isPosProductAvailable(p);
        const stockLabel = formatPosStock(p);
        const byWeight = isSellByWeight(p);
        const isService = p.isService || p.itemType === 'Service';
        const weightClass = byWeight ? ' is-weighed' : '';
        const selectedClass = (byWeight && lastTappedProductId === p.id) ? ' is-scale-selected' : '';
        const weighBtn = byWeight
            ? `<button type="button" class="btn-pos-weigh" data-weigh-id="${p.id}">${tr('weigh_btn')}</button>`
            : '';
        const stockClass = !available ? 'pos-oos-label' : (isService ? 'pos-service-label' : '');
        return `<div class="pos-product-card ${available ? '' : 'is-out-of-stock'}${weightClass}${selectedClass}${isService ? ' is-service' : ''}" data-id="${p.id}" data-available="${available ? '1' : '0'}">
            <div class="pos-product-image placeholder"><span class="material-symbols-rounded">inventory_2</span>
                ${img ? `<img src="${escapeHtml(img)}" alt="" onload="this.parentElement.classList.remove('placeholder');this.previousElementSibling.style.display='none';" onerror="this.remove();">` : ''}
            </div>
            <h3>${escapeHtml(p.name)}</h3>
            <p class="${stockClass}">${escapeHtml(stockLabel)}</p>
            <div class="price">${formatPosPrice(p)}</div>
            ${weighBtn}
        </div>`;
    }).join('') : `<div class="empty-state pos-empty-products"><span class="material-symbols-rounded">inventory_2</span><div>${tr('no_products')}</div></div>`;

    grid.querySelectorAll('[data-weigh-id]').forEach(btn => {
        btn.onclick = async (e) => {
            e.stopPropagation();
            const id = Number(btn.dataset.weighId);
            const p = products.find(x => x.id === id);
            if (!p) return;
            if (!isPosProductAvailable(p)) {
                if (await showOutOfStockPopup(p.name)) {
                    addToCart(id, 1, { allowZeroStock: true, skipStock: true });
                }
                return;
            }
            scaleManager.selectForWeighing(p);
            renderPOS();
            document.getElementById('scaleManualWeight')?.focus();
        };
    });

    grid.querySelectorAll('.pos-product-card').forEach(card => {
        card.onclick = async () => {
            const id = Number(card.dataset.id);
            const p = products.find(x => x.id === id);
            if (!isPosProductAvailable(p)) {
                if (await showOutOfStockPopup(p?.name)) {
                    if (isSellByWeight(p)) {
                        scaleManager.selectForWeighing(p);
                        renderPOS();
                        document.getElementById('scaleManualWeight')?.focus();
                    } else {
                        addToCart(id, 1, { allowZeroStock: true, skipStock: true });
                    }
                }
                return;
            }
            if (isSellByWeight(p)) {
                // Weight products: same as Weigh button (never add full $/kg as 1 pc)
                scaleManager.selectForWeighing(p);
                renderPOS();
                document.getElementById('scaleManualWeight')?.focus();
                return;
            }
            addToCart(id);
        };
    });
    populatePosCustomer();
    populatePosCurrency();
    updateShippingButton();
    renderCart();
}

function filteredProducts(search, cat, opts = {}) {
    const q = (search || '').toLowerCase().trim();
    const { forPos = false, invMode = false } = opts;
    return products.filter(p => {
        if (forPos && (p.isInactive || p.status === 'Inactive')) return false;
        if (invMode && invFilter === 'active' && (p.isInactive || p.status === 'Inactive')) return false;
        if (invMode && invFilter === 'low' && !(p.isStockTracked !== false && p.stock <= (p.minStock || 0))) return false;
        const matchCat = cat === 'all' || p.category === cat;
        const matchQ = !q || [p.name, p.sku, p.barcode, p.category, p.location].some(x => (x || '').toLowerCase().includes(q));
        return matchCat && matchQ;
    });
}

function getCategoryIcon(catName) {
    const s = String(catName || '').toLowerCase();
    if (s.includes('aged') || s.includes('معتق') || s.includes('specialty')) return 'workspace_premium';
    if (s.includes('milk') || s.includes('حليب') || s.includes('laban') || s.includes('لبن') || s.includes('عيران') || s.includes('ayran')) return 'local_drink';
    if (s.includes('cheese') || s.includes('جبن') || s.includes('عكاوي') || s.includes('حلوم') || s.includes('نابلسي')) return 'lunch_dining';
    if (s.includes('butter') || s.includes('ghee') || s.includes('سمن') || s.includes('زبد') || s.includes('قشطة') || s.includes('cream')) return 'bakery_dining';
    if (s.includes('yogurt') || s.includes('زبادي') || s.includes('لبنة') || s.includes('labneh')) return 'icecream';
    if (s.includes('sweet') || s.includes('حلويات') || s.includes('dessert') || s.includes('كنافة')) return 'cake';
    if (s.includes('pack') || s.includes('تغليف') || s.includes('علب') || s.includes('أكياس')) return 'inventory_2';
    return 'category';
}

function productThumb(p) {
    const img = p.image || p.categoryImage || '';
    if (img && !img.includes('nuricon_inventory')) {
        return `<img class="inv-thumb" src="${escapeHtml(img)}" alt="" onerror="this.style.display='none'">`;
    }
    const icon = getCategoryIcon(p.category);
    return `<span class="inv-thumb" style="display:inline-flex;align-items:center;justify-content:center;"><span class="material-symbols-rounded" style="opacity:.4;">${icon}</span></span>`;
}

function updateInvBulkBtn() {
    const btn = document.getElementById('btn-bulk-delete-inventory');
    if (!btn) return;
    btn.hidden = invSelected.size === 0;
    if (typeof updateInvActionsSlide === 'function') updateInvActionsSlide();
}

function renderInventory() {
    renderCategoryPills('inv-cats', invCat, c => { invCat = c; renderInventory(); });
    const list = filteredProducts(document.getElementById('inv-search')?.value, invCat, { invMode: true });
    const canEdit = can('admin');
    const tableWrap = document.getElementById('inv-table-wrap');
    const cardsEl = document.getElementById('inv-cards');
    if (tableWrap) {
        const showTable = invView === 'table';
        tableWrap.hidden = !showTable;
        if (showTable) tableWrap.removeAttribute('hidden');
        else tableWrap.setAttribute('hidden', '');
    }
    if (cardsEl) {
        const showCards = invView === 'card';
        cardsEl.hidden = !showCards;
        if (showCards) cardsEl.removeAttribute('hidden');
        else cardsEl.setAttribute('hidden', '');
    }

    document.getElementById('btn-inv-table')?.classList.toggle('active', invView === 'table');
    document.getElementById('btn-inv-cards')?.classList.toggle('active', invView === 'card');

    if (invView === 'table') {
        const body = document.getElementById('inv-body');
        if (!body) return;
        body.innerHTML = list.length ? list.map(p => `
            <tr class="${p.isInactive ? 'inv-row-inactive' : ''}">
                <td>${canEdit ? `<input type="checkbox" class="inv-row-check" data-inv-id="${p.id}" ${invSelected.has(p.id) ? 'checked' : ''}>` : ''}</td>
                <td>${productThumb(p)}</td>
                <td><strong>${escapeHtml(p.name)}</strong>${isSellByWeight(p) ? ` <span class="badge badge-weight">${tr('weight_badge')}</span>` : ''}${p.isInactive ? ` <span class="badge out-of-stock">${tr('inactive')}</span>` : ''}</td>
                <td>${escapeHtml(p.sku || '—')}</td>
                <td>${escapeHtml(p.barcode || '—')}</td>
                <td>${escapeHtml(p.category || '—')}</td>
                <td>${isSellByWeight(p) ? formatPosPrice(p) : money(p.price)}</td>
                <td>${p.isStockTracked === false ? '—' : (isSellByWeight(p) ? `${((Number(p.stock)||0)/1000).toFixed(3)} ${tr('unit_kg')}` : p.stock)}</td>
                <td>${p.minStock ?? 0}</td>
                <td>${escapeHtml(p.location || '—')}</td>
                <td>${stockBadge(p)}${isSellByWeight(p) ? ` <span class="badge badge-weight">${tr('weight_badge')}</span>` : ''}</td>
                <td>${canEdit ? actionBtns(`data-edit-product="${p.id}"`, `data-del-product="${p.id}"`,
                    `<button type="button" class="btn-icon" title="${tr('adjust_stock')}" data-adjust-product="${p.id}"><span class="material-symbols-rounded">inventory_2</span></button>`) : '—'}</td>
            </tr>`).join('')
            : `<tr><td colspan="12" class="empty-state">${tr('empty_list')}</td></tr>`;

        const selectAll = document.getElementById('inv-select-all');
        if (selectAll) {
            selectAll.checked = list.length > 0 && list.every(p => invSelected.has(p.id));
            selectAll.indeterminate = list.some(p => invSelected.has(p.id)) && !selectAll.checked;
        }
        document.querySelectorAll('.inv-row-check').forEach(cb => {
            cb.onchange = () => {
                const id = Number(cb.dataset.invId);
                if (cb.checked) invSelected.add(id); else invSelected.delete(id);
                updateInvBulkBtn();
                const sa = document.getElementById('inv-select-all');
                if (sa) sa.checked = list.length > 0 && list.every(p => invSelected.has(p.id));
            };
        });
    } else if (cardsEl) {
        const cards = list.map(p => `
            <div class="inv-card ${p.isInactive ? 'inactive' : ''}" data-inv-card="${p.id}">
                ${canEdit ? `<input type="checkbox" class="inv-card-check inv-row-check" data-inv-id="${p.id}" ${invSelected.has(p.id) ? 'checked' : ''}>` : ''}
                ${productThumb(p).replace('inv-thumb', 'inv-thumb inv-card-img')}
                <h3 title="${escapeHtml(p.name)}">${escapeHtml(p.name)}</h3>
                <div class="inv-card-meta" title="${escapeHtml(p.sku || '—')} · ${escapeHtml(p.category || '—')}">${escapeHtml(p.sku || '—')} · ${escapeHtml(p.category || '—')}</div>
                <div class="inv-card-meta">${tr('col_stock')}: ${p.isStockTracked === false ? '—' : (isSellByWeight(p) ? `${((Number(p.stock)||0)/1000).toFixed(3)} ${tr('unit_kg')}` : p.stock)}</div>
                <div class="price">${isSellByWeight(p) ? formatPosPrice(p) : money(p.price)}</div>
                <div class="inv-card-badges">
                    ${isSellByWeight(p) ? `<span class="badge badge-weight">${tr('weight_badge')}</span>` : ''}
                    ${stockBadge(p)}
                </div>
            </div>`).join('');
        const addCard = canEdit ? `<div class="inv-card inv-card-add" id="inv-card-add"><span class="material-symbols-rounded">add</span><span data-i18n="add_new">Add New</span></div>` : '';
        cardsEl.innerHTML = (cards + addCard) || `<div class="empty-state">${tr('empty_list')}</div>`;
        cardsEl.querySelectorAll('[data-inv-card]').forEach(card => {
            card.onclick = e => {
                if (e.target.closest('.inv-row-check')) return;
                openProductModal(Number(card.dataset.invCard));
            };
        });
        cardsEl.querySelectorAll('.inv-row-check').forEach(cb => {
            cb.onclick = e => e.stopPropagation();
            cb.onchange = () => {
                const id = Number(cb.dataset.invId);
                if (cb.checked) invSelected.add(id); else invSelected.delete(id);
                updateInvBulkBtn();
            };
        });
        document.getElementById('inv-card-add')?.addEventListener('click', () => openProductModal(null));
        applyI18n();
    }

    document.querySelectorAll('[data-edit-product]').forEach(btn => btn.onclick = () => openProductModal(Number(btn.dataset.editProduct)));
    document.querySelectorAll('[data-del-product]').forEach(btn => btn.onclick = () => deleteProduct(Number(btn.dataset.delProduct)));
    document.querySelectorAll('[data-adjust-product]').forEach(btn => btn.onclick = () => openStockModal(Number(btn.dataset.adjustProduct)));
    updateInvBulkBtn();
    requestAnimationFrame(() => {
        if (typeof updateInvActionsSlide === 'function') updateInvActionsSlide();
    });
}

function renderPosStats() {
    const d = dashboard || {};
    const ordersEl = document.getElementById('pos-stat-orders');
    const salesEl = document.getElementById('pos-stat-sales');
    const pendingEl = document.getElementById('pos-stat-pending');
    if (ordersEl) ordersEl.textContent = String(d.ordersToday ?? 0);
    if (salesEl) {
        const s = Number(d.todaySales) || 0;
        salesEl.textContent = money(Math.max(0, s));
    }
    if (pendingEl) pendingEl.textContent = String(d.pendingOrders ?? 0);
}

function renderReports() {
    const s = reportSummary || {};
    const d = dashboard || {};
    const grid = document.getElementById('report-stats');
    if (!grid) return;
    const rangeLabel = (s.fromDate && s.toDate)
        ? `${toInputDate(s.fromDate)} → ${toInputDate(s.toDate)}`
        : '';
    const cards = [
        { label: tr('rep_sales'), value: money(s.totalSales), sub: rangeLabel, cls: '' },
        { label: tr('rep_cost'), value: money(s.totalCost), sub: '', cls: '' },
        { label: tr('rep_expenses'), value: money(s.totalExpenses), sub: '', cls: '' },
        { label: tr('rep_profit_before_expenses'), value: money(s.totalProfit), sub: '', cls: 'stat-profit' },
        { label: tr('rep_profit_after_expenses'), value: money(s.totalProfitAfterExpenses), sub: '', cls: 'stat-profit-net' },
        { label: tr('pos_orders'), value: String(d.ordersToday ?? 0), sub: tr('orders_today'), cls: '' },
        { label: tr('pos_pending'), value: String(d.pendingOrders ?? 0), sub: '', cls: '' },
    ];
    if (Number(s.totalMilkLiters || 0) > 0 || Number(s.totalMilkExpenses || 0) > 0) {
        cards.push({ label: tr('col_liters'), value: Number(s.totalMilkLiters || 0).toLocaleString() + ' L', sub: tr('exp_filter_milk'), cls: 'stat-milk' });
        cards.push({ label: tr('milk_cost'), value: money(s.totalMilkExpenses || 0), sub: '', cls: 'stat-milk' });
        if (Number(s.totalUnpaidMilkExpenses || 0) > 0) {
            cards.push({ label: tr('unpaid_milk_balance'), value: money(s.totalUnpaidMilkExpenses || 0), sub: tr('due_to_deliverers'), cls: 'stat-due' });
        }
    }
    const seen = new Set();
    grid.innerHTML = cards.filter(c => {
        const key = String(c.label || '').trim().toLowerCase();
        if (!key || seen.has(key)) return false;
        seen.add(key);
        return true;
    }).map(c => `
        <div class="stat-card ${c.cls || ''}"><div class="label">${escapeHtml(c.label)}</div><div class="value">${c.value}</div>
            ${c.sub ? `<div class="stat-sub">${escapeHtml(c.sub)}</div>` : ''}</div>`).join('');
    const rows = reportTop || [];
    document.getElementById('report-top-body').innerHTML = rows.length ? rows.map(r => `
        <tr><td>${escapeHtml(r.product_name || r.ProductName || '')}</td>
        <td>${r.quantity_sold ?? r.QuantitySold ?? 0}</td>
        <td>${money(r.total_sales ?? r.TotalSales)}</td>
        <td>${money(r.profit ?? r.Profit)}</td></tr>`).join('')
        : `<tr><td colspan="4" class="empty-state">${tr('empty_list')}</td></tr>`;

    // Render Milk Deliverers Breakdown
    const milkRows = reportExpensesBreakdown?.milkSuppliers || [];
    const milkTbody = document.getElementById('report-milk-body');
    if (milkTbody) {
        milkTbody.innerHTML = milkRows.length ? milkRows.map(m => `
            <tr>
                <td><strong>${escapeHtml(m.supplier_name || tr('unassigned'))}</strong></td>
                <td>${m.delivery_count ?? 0}</td>
                <td><strong>${Number(m.total_liters ?? 0).toLocaleString()} L</strong></td>
                <td>${money(m.avg_unit_price ?? 0)}</td>
                <td><strong>${money(m.total_amount ?? 0)}</strong></td>
                <td style="color:#16a34a; font-weight:600;">${money(m.total_paid ?? 0)}</td>
                <td style="color:${Number(m.total_unpaid || 0) > 0 ? '#ea580c; font-weight:700;' : 'inherit;'}">${money(m.total_unpaid ?? 0)}</td>
                <td>
                    <button type="button" class="btn btn-secondary btn-sm" data-rep-statement="${m.supplier_id}">
                        <span class="material-symbols-rounded" style="font-size:16px;">receipt_long</span>
                        ${tr('print_statement')}
                    </button>
                </td>
            </tr>
        `).join('') : `<tr><td colspan="8" class="empty-state">${tr('empty_list')}</td></tr>`;

        milkTbody.querySelectorAll('[data-rep-statement]').forEach(btn => {
            btn.onclick = () => {
                const suppId = Number(btn.dataset.repStatement) || null;
                const from = document.getElementById('report-from')?.value || '';
                const to = document.getElementById('report-to')?.value || '';
                openMilkStatement(suppId, from, to);
            };
        });
    }

    // Render Expense Categories Breakdown
    const catRows = reportExpensesBreakdown?.categories || [];
    const catTbody = document.getElementById('report-cats-body');
    if (catTbody) {
        catTbody.innerHTML = catRows.length ? catRows.map(c => `
            <tr>
                <td><strong>${escapeHtml(c.category)}</strong></td>
                <td>${c.expense_count ?? 0}</td>
                <td><strong>${money(c.total_amount ?? 0)}</strong></td>
                <td style="color:#16a34a; font-weight:600;">${money(c.paid_amount ?? 0)}</td>
                <td style="color:${Number(c.unpaid_amount || 0) > 0 ? '#ea580c; font-weight:700;' : 'inherit;'}">${money(c.unpaid_amount ?? 0)}</td>
            </tr>
        `).join('') : `<tr><td colspan="5" class="empty-state">${tr('empty_list')}</td></tr>`;
    }
}

function updateShippingButton() {
    const btn = document.getElementById('btn-add-shipping');
    if (!btn) return;
    btn.textContent = posShipping?.shippingTo ? tr('view_shipping') : tr('add_shipping');
}

function populatePosCustomer() {
    const sel = document.getElementById('pos-customer');
    if (!sel) return;
    const cur = sel.value;
    sel.innerHTML = `<option value="">${escapeHtml(tr('walk_in_customer'))}</option>` +
        customers.map(c => `<option value="${c.id}">${escapeHtml(c.name)}</option>`).join('');
    if (cur && [...sel.options].some(o => o.value === cur)) sel.value = cur;
    updatePosCustomerDebt();
}

function updatePosCustomerDebt() {
    const el = document.getElementById('pos-customer-debt');
    if (!el) return;
    const id = Number(document.getElementById('pos-customer')?.value || 0);
    if (!id) {
        el.hidden = true;
        el.innerHTML = '';
        return;
    }
    const c = customers.find(x => x.id === id);
    if (!c) {
        el.hidden = true;
        return;
    }
    const bal = Number(c.balance) || 0;
    const limit = Number(c.creditLimit);
    const hasLimit = Number.isFinite(limit) && limit > 0;
    const left = hasLimit ? Math.max(0, limit - bal) : null;
    el.hidden = false;
    el.innerHTML = `<span class="pos-debt-owed ${bal > 0 ? 'has-debt' : ''}">${tr('pos_amount_owed')}: <strong>${money(bal)}</strong></span>` +
        (left != null ? `<span class="pos-debt-credit">${tr('pos_credit_left')}: <strong>${money(left)}</strong></span>` : '');
}

function populatePosCurrency() {
    const sel = document.getElementById('pos-currency');
    if (!sel) return;
    const cur = sel.value || 'USD';
    const list = (currencies && currencies.length)
        ? currencies
        : [{ code: 'USD', symbol: '$', rate: 1, name: 'US Dollar' }];
    sel.innerHTML = list.map(c => {
        const code = (c.code || c.Code || '').toString().trim() || 'USD';
        return `<option value="${escapeHtml(code)}">${escapeHtml(code)}</option>`;
    }).join('');
    if ([...sel.options].some(o => o.value === cur)) sel.value = cur;
    else sel.value = list[0]?.code || list[0]?.Code || 'USD';
    if (!sel.value) sel.value = 'USD';
}

function normalizeScanCode(code) {
    return String(code || '')
        .replace(/[\u0000-\u001F\u007F]/g, '') // control chars from some scanners
        .replace(/\u200e|\u200f/g, '')
        .trim();
}

function findProductByScan(code) {
    const q = normalizeScanCode(code).toLowerCase();
    if (!q) return null;
    const norm = (v) => normalizeScanCode(v).toLowerCase();
    // Exact barcode / SKU first
    let p = products.find(x => norm(x.barcode) && norm(x.barcode) === q)
        || products.find(x => norm(x.sku) && norm(x.sku) === q);
    if (p) return p;
    // Match without leading zeros
    const qStrip = q.replace(/^0+/, '') || q;
    p = products.find(x => {
        const b = norm(x.barcode).replace(/^0+/, '') || norm(x.barcode);
        const s = norm(x.sku).replace(/^0+/, '') || norm(x.sku);
        return (b && b === qStrip) || (s && s === qStrip);
    });
    if (p) return p;
    // Exact name last
    return products.find(x => norm(x.name) === q) || null;
}

let posScanBuffer = '';
let posScanTimer = null;
let posSearchDebounce = null;
let posScanLastKeyAt = 0;
let posScanFastGaps = 0;

function isPosViewActive() {
    return !!document.getElementById('pos')?.classList.contains('active');
}

function scrollPosCartIntoView() {
    const panel = document.querySelector('#pos .pos-order-panel');
    if (!panel) return;
    // On stacked mobile layout the cart sits below products
    panel.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
}

async function tryPosScanAdd(codeOverride) {
    const input = document.getElementById('pos-search');
    const code = normalizeScanCode(codeOverride != null ? codeOverride : (input?.value || ''));
    if (!code) return;

    // TM-A17 / EAN-13 scale printed labels first
    try {
        const handled = await scaleManager.resolveBarcode(code);
        if (handled) {
            if (input) input.value = '';
            renderPOS();
            scrollPosCartIntoView();
            input?.focus();
            return;
        }
    } catch { /* fall through to normal scan */ }

    const p = findProductByScan(code);
    if (!p) {
        if (input) input.value = '';
        renderPOS();
        toast(tr('item_not_found') + ': ' + code, 'error');
        input?.focus();
        return;
    }
    if (!isPosProductAvailable(p)) {
        if (input) input.value = '';
        renderPOS();
        await showOutOfStockPopup(p.name);
        input?.focus();
        return;
    }
    if (addToCart(p.id)) {
        if (input) input.value = '';
        renderPOS();
        scrollPosCartIntoView();
        toast(`${p.name} ✓`, 'success');
    } else if (input) {
        input.value = '';
        renderPOS();
    }
    input?.focus();
}

function handlePosScannerKeydown(e) {
    if (!isPosViewActive()) return;
    if (e.ctrlKey || e.altKey || e.metaKey) return;

    const t = e.target;
    const tag = (t?.tagName || '').toLowerCase();
    const onPosSearch = t?.id === 'pos-search';
    const typingElsewhere = !onPosSearch && (
        tag === 'textarea' || tag === 'select' ||
        (tag === 'input' && t.type !== 'checkbox' && t.type !== 'radio' && t.type !== 'button' && t.type !== 'submit') ||
        t?.isContentEditable
    );
    if (typingElsewhere) return;

    // Enter ends a scan
    if (e.key === 'Enter') {
        const fromBuffer = normalizeScanCode(posScanBuffer);
        const fromInput = onPosSearch ? normalizeScanCode(t.value) : '';
        const code = fromBuffer.length >= 2 ? fromBuffer : fromInput;
        posScanBuffer = '';
        if (posScanTimer) { clearTimeout(posScanTimer); posScanTimer = null; }
        if (!code) return;
        e.preventDefault();
        e.stopPropagation();
        tryPosScanAdd(code);
        return;
    }

    // Build buffer for wedge scanners (works even when search isn't focused)
    if (e.key.length === 1) {
        const now = Date.now();
        if (posScanBuffer && (now - posScanLastKeyAt) < 55) posScanFastGaps++;
        else posScanFastGaps = 0;
        posScanLastKeyAt = now;
        posScanBuffer += e.key;
        if (posScanTimer) clearTimeout(posScanTimer);
        // Auto-submit only for scanner-speed bursts (no Enter suffix required)
        posScanTimer = setTimeout(() => {
            const code = normalizeScanCode(posScanBuffer);
            const wasScanner = posScanFastGaps >= 3 && code.length >= 4;
            posScanBuffer = '';
            posScanFastGaps = 0;
            if (wasScanner && isPosViewActive()) tryPosScanAdd(code);
        }, 90);
    } else if (e.key === 'Backspace') {
        posScanBuffer = posScanBuffer.slice(0, -1);
        posScanFastGaps = 0;
    }
}

function renderCart() {
    clearPosTotalManual();
    const el = document.getElementById('cart-items');
    if (!el) return;
    if (!cart.length) el.innerHTML = `<div class="empty-state">${tr('empty_cart_hint')}</div>`;
    else {
        el.innerHTML = cart.map((item, i) => {
            const tiers = (item.prices && item.prices.length) ? item.prices : [item.price];
            const tierOpts = tiers.map((pr, ti) => {
                if (!(Number(pr) > 0) && ti > 0) return '';
                const sel = Math.abs(Number(pr) - Number(item.price)) < 0.0001 ? ' selected' : '';
                return `<option value="${ti}"${sel}>P${ti + 1}</option>`;
            }).join('');
            const isWeight = item.weighted || isSellByWeight(item);
            const stepVal = isWeight ? '0.001' : '1';
            const minVal = isWeight ? '0.001' : '1';
            const formattedQty = isWeight ? Number(item.qty).toFixed(3).replace(/\.?0+$/, '') || String(item.qty) : String(item.qty);

            return `<div class="cart-item" data-i="${i}">
                <div class="cart-item-top">
                    <div class="cart-item-info"><strong>${escapeHtml(item.name)}</strong>${item.skipStock || item.custom ? `<span class="cart-item-badge">${escapeHtml(tr('quick_sale_badge'))}</span>` : ''}</div>
                    <strong class="cart-line-total">${posMoney(item.price * item.qty)}</strong>
                </div>
                <div class="cart-item-controls">
                    <div class="qty-controls">
                        <button type="button" data-qty="${i}" data-d="-1">−</button>
                        <input type="number" class="cart-qty-input" data-qty-input="${i}" min="${minVal}" step="${stepVal}" value="${formattedQty}">
                        <button type="button" data-qty="${i}" data-d="1">+</button>
                    </div>
                    <div class="cart-price-wrap">
                        <select class="cart-tier" data-tier="${i}" title="${tr('prices')}">${tierOpts}</select>
                        <input type="number" class="cart-price-input" data-price="${i}" min="0.01" step="0.01" value="${Number(item.price).toFixed(2)}">
                    </div>
                    <button type="button" class="cart-remove" data-rm="${i}" title="${tr('remove_line')}"><span class="material-symbols-rounded">delete</span></button>
                </div>
            </div>`;
        }).join('');
        el.querySelectorAll('button[data-qty]').forEach(btn => {
            btn.onclick = () => {
                const i = Number(btn.dataset.qty), d = Number(btn.dataset.d);
                if (!cart[i]) return;
                const isWeight = cart[i].weighted || isSellByWeight(cart[i]);
                const delta = isWeight ? (d > 0 ? 0.1 : -0.1) : d;
                cart[i].qty = Math.max(0, Number((cart[i].qty + delta).toFixed(3)));
                if (cart[i].qty <= 0) cart.splice(i, 1);
                else if (cart[i].qty > cart[i].max) cart[i].qty = cart[i].max;
                renderCart();
            };
        });
        el.querySelectorAll('input[data-qty-input]').forEach(inp => {
            const commitQty = () => {
                const i = Number(inp.dataset.qtyInput);
                if (!cart[i]) return;
                const next = parseFloat(inp.value);
                if (!Number.isFinite(next) || next <= 0) { inp.value = String(cart[i].qty); return; }
                cart[i].qty = Math.min(cart[i].max, next);
                renderCart();
            };
            inp.onchange = commitQty;
            inp.onblur = commitQty;
            inp.onkeydown = (e) => {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    commitQty();
                }
            };
        });
        el.querySelectorAll('select[data-tier]').forEach(sel => {
            sel.onchange = () => {
                const i = Number(sel.dataset.tier);
                const ti = Number(sel.value);
                const pr = Number(cart[i].prices?.[ti]);
                if (pr > 0) cart[i].price = pr;
                renderCart();
            };
        });
        el.querySelectorAll('input[data-price]').forEach(inp => {
            inp.onchange = () => {
                const i = Number(inp.dataset.price);
                const v = parseFloat(inp.value);
                if (!(v > 0)) { toast(tr('invalid_price'), 'error'); inp.value = Number(cart[i].price).toFixed(2); return; }
                cart[i].price = v;
                renderCart();
            };
        });
        el.querySelectorAll('button[data-rm]').forEach(btn => {
            btn.onclick = () => { cart.splice(Number(btn.dataset.rm), 1); renderCart(); };
        });
    }
    updatePosTotalsUi();
}

function updatePosTotalsUi() {
    const t = getPosTotals();
    const shipWrap = document.getElementById('pos-ship-wrap');
    const discWrap = document.getElementById('pos-disc-wrap');
    if (shipWrap) shipWrap.hidden = !t.shipOn;
    if (discWrap) discWrap.hidden = !t.discOn;
    const set = (id, v) => { const n = document.getElementById(id); if (n) n.textContent = posMoney(v); };
    set('cart-subtotal', t.sub);
    set('cart-vat', t.vat);
    set('cart-ship', t.ship);
    set('cart-disc', t.disc);
    const { symbol, code } = posCurrencyMeta();
    const sym = document.getElementById('cart-total-sym');
    if (sym) sym.textContent = symbol;
    const totalInp = document.getElementById('pos-total-amt');
    const base = t.manual ? t.total : t.calc;
    if (totalInp && document.activeElement !== totalInp) {
        totalInp.value = formatPosAmount(base);
        totalInp.dataset.currency = code;
    }
    const edit = totalInp?.closest('.pos-total-edit');
    if (edit) edit.classList.toggle('is-lbp', code === 'LBP');
}

function clearPosTotalManual() {
    posTotalManual = null;
}

function renderSales() {
    document.getElementById('sales-body').innerHTML = sales.length ? sales.map(o => {
        const isPaid = String(o.paymentStatus || 'Paid').toLowerCase() === 'paid';
        return `<tr><td>#${o.orderId}</td><td>${formatDate(o.date)}</td><td>${escapeHtml(o.customer)}</td>
        <td>${money(o.total)}</td>
        <td><span class="badge ${isPaid ? 'in-stock' : 'low-stock'}">${isPaid ? tr('paid') : tr('unpaid')}</span></td>
        <td><div class="table-actions">
            <button type="button" class="btn btn-secondary btn-sm" data-view-order="${o.orderId}">${tr('view')}</button>
            <button type="button" class="btn btn-secondary btn-sm" data-return="${o.orderId}">${tr('return')}</button>
        </div></td></tr>`;
    }).join('')
        : `<tr><td colspan="6" class="empty-state">${tr('empty_list')}</td></tr>`;
    document.querySelectorAll('[data-return]').forEach(btn => btn.onclick = () => openReturn(Number(btn.dataset.return)));
    document.querySelectorAll('[data-view-order]').forEach(btn => btn.onclick = () => viewOrder(Number(btn.dataset.viewOrder)));
}

function historyColLabel(key) {
    const k = String(key || '').trim();
    const map = {
        items: 'col_items', Items: 'col_items', ITEMS: 'col_items',
        status: 'col_status', Status: 'col_status', STATUS: 'col_status',
        total: 'col_total', Total: 'col_total', TOTAL: 'col_total',
        customer: 'col_customer', Customer: 'col_customer', CUSTOMER: 'col_customer',
        date: 'col_date', Date: 'col_date', DATE: 'col_date',
        'order id': 'col_order_id', 'Order Id': 'col_order_id', 'Order ID': 'col_order_id', 'ORDER ID': 'col_order_id',
        orderId: 'col_order_id', OrderId: 'col_order_id',
        name: 'col_name', Name: 'col_name', NAME: 'col_name',
        sku: 'col_sku', SKU: 'col_sku',
        category: 'col_category', Category: 'col_category', CATEGORY: 'col_category',
        phone: 'col_phone', Phone: 'col_phone', PHONE: 'col_phone',
        email: 'col_email', Email: 'col_email', EMAIL: 'col_email',
        amount: 'col_amount', Amount: 'col_amount', AMOUNT: 'col_amount',
        description: 'col_desc', Description: 'col_desc', DESCRIPTION: 'col_desc',
        payment: 'col_payment', Payment: 'col_payment', PAYMENT: 'col_payment',
    };
    const i18nKey = map[k] || map[k.toLowerCase()];
    return i18nKey ? tr(i18nKey) : k;
}

function renderHistory() {
    const rows = window._historyRows || [];
    const head = document.getElementById('history-head');
    const body = document.getElementById('history-body');
    if (!rows.length) {
        head.innerHTML = '';
        body.innerHTML = `<tr><td class="empty-state">${tr('empty_list')}</td></tr>`;
        return;
    }
    const keys = Object.keys(rows[0]);
    head.innerHTML = `<tr>${keys.map(k => `<th>${escapeHtml(historyColLabel(k))}</th>`).join('')}</tr>`;
    body.innerHTML = rows.slice(0, 100).map(r => `<tr>${keys.map(k => {
        let v = r[k];
        if (v && (String(k).toLowerCase().includes('date') || String(k).toLowerCase().includes('timestamp'))) v = formatDate(v);
        else if (typeof v === 'number' && (String(k).toLowerCase().includes('total') || String(k).toLowerCase().includes('amount'))) v = money(v);
        return `<td>${escapeHtml(v)}</td>`;
    }).join('')}</tr>`).join('');
}

function renderQuotations() {
    document.getElementById('quot-body').innerHTML = quotations.length ? quotations.map(q => `
        <tr><td>#${q.orderId}</td><td>${formatDate(q.orderDate)}</td><td>${escapeHtml(q.customerName)}</td>
        <td>${money(q.totalAmount)}</td>
        <td><div class="table-actions">
            <button type="button" class="btn-icon btn-icon-bare" title="${tr('preview')}" data-preview-quot="${q.orderId}"><span class="material-symbols-rounded">visibility</span></button>
            <button type="button" class="btn-icon btn-icon-success" title="${tr('convert')}" data-convert="${q.orderId}"><span class="material-symbols-rounded">check</span></button>
            <button type="button" class="btn-icon btn-icon-danger" title="${tr('delete')}" data-del-quot="${q.orderId}"><span class="material-symbols-rounded">delete</span></button>
        </div></td></tr>`).join('')
        : `<tr><td colspan="5" class="empty-state">${tr('empty_list')}</td></tr>`;
    document.querySelectorAll('[data-preview-quot]').forEach(btn => {
        btn.onclick = () => openQuotationPreview(Number(btn.dataset.previewQuot));
    });
    document.querySelectorAll('[data-convert]').forEach(btn => {
        btn.onclick = async () => {
            try {
                await api('/api/quotations/' + btn.dataset.convert + '/convert', { method: 'POST' });
                toast(tr('convert_ok'), 'success'); await loadData();
            } catch (e) { toast(e.message, 'error'); }
        };
    });
    document.querySelectorAll('[data-del-quot]').forEach(btn => btn.onclick = () => deleteQuotation(Number(btn.dataset.delQuot)));
}

let _quotePreviewData = null;

function formatQuoteDate(d) {
    try {
        const dt = new Date(d);
        if (Number.isNaN(dt.getTime())) return String(d || '');
        return dt.toLocaleDateString(lang === 'ar' ? 'ar' : 'en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
    } catch { return String(d || ''); }
}

function quoteBrandDisplay(name) {
    const raw = (name || 'Al-Tayebat').trim();
    const word = raw.split(/\s+/)[0] || raw;
    return word.toUpperCase();
}

function quoteImageHtml(path, name) {
    const img = (path || '').trim();
    if (img) {
        const url = img.startsWith('/') || img.startsWith('http') ? img : '/' + img.replace(/^\/+/, '');
        return `<img class="quote-item-img" src="${escapeHtml(url)}" alt="" onerror="this.style.display='none'">`;
    }
    return '';
}

async function openQuotationPreview(orderId) {
    try {
        const data = await api('/api/quotations/' + orderId);
        _quotePreviewData = data;
        const title = document.getElementById('quote-preview-title');
        if (title) title.textContent = `${tr('quote_preview_title')} - #${data.orderId}`;
        const company = document.getElementById('quote-company-name');
        if (company) company.textContent = quoteBrandDisplay(data.companyName);
        const info = document.getElementById('quote-company-info');
        if (info) info.textContent = data.companyInfo || '';
        const meta = document.getElementById('quote-meta-line');
        if (meta) {
            meta.textContent = tr('quote_preview_meta')
                .replace('{0}', data.orderId)
                .replace('{1}', formatQuoteDate(data.orderDate || new Date()))
                .replace('{2}', data.customerId != null ? data.customerId : 'N/A')
                .replace('{3}', tr('quote_validity_days'));
        }
        const cust = document.getElementById('quote-cust-body');
        if (cust) {
            const name = data.customerName || tr('walk_in_customer');
            const address = (data.address || '').trim() || tr('quote_no_address');
            const phone = (data.phone || '').trim() || tr('quote_no_phone');
            cust.innerHTML = `<div class="quote-cust-name">${escapeHtml(name)}</div>
                <div class="quote-cust-line">${escapeHtml(tr('col_address'))}: ${escapeHtml(address)} | ${escapeHtml(tr('col_phone'))}: ${escapeHtml(phone)}</div>`;
        }
        const body = document.getElementById('quote-items-body');
        const items = data.items || [];
        if (body) {
            body.innerHTML = items.length ? items.map(it => {
                const lineTotal = Number(it.price) * Number(it.qty);
                const desc = [it.name, it.description].filter(Boolean).map(escapeHtml).join('<br>');
                return `<tr>
                    <td class="quote-photo-cell">${quoteImageHtml(it.image, it.name)}</td>
                    <td>${desc}</td>
                    <td>${it.qty}</td>
                    <td>${money(it.price)}</td>
                    <td><strong>${money(lineTotal)}</strong></td>
                </tr>`;
            }).join('') : `<tr><td colspan="5" class="empty-state">${tr('empty_list')}</td></tr>`;
        }
        const subtotal = Number(data.subtotal || 0);
        const grand = Number(data.totalAmount != null ? data.totalAmount : subtotal);
        const tax = Math.max(0, grand - subtotal);
        const totals = document.getElementById('quote-totals');
        if (totals) {
            totals.innerHTML = `
                <div class="quote-total-row"><span>${tr('subtotal')}</span><strong>${money(subtotal)}</strong></div>
                ${tax > 0.009 ? `<div class="quote-total-row"><span>${tr('quote_tax_extras')}</span><strong>${money(tax)}</strong></div>` : ''}
                <div class="quote-total-row quote-total-grand"><span>${tr('grand_total')}</span><strong>${money(grand)}</strong></div>`;
        }
        openModal('quotation-preview-modal');
    } catch (e) { toast(e.message, 'error'); }
}

function printQuotationPreview() {
    const sheet = document.getElementById('quote-preview-sheet');
    if (!sheet) return;
    document.body.classList.add('printing-quote');
    const cleanup = () => {
        document.body.classList.remove('printing-quote');
        window.removeEventListener('afterprint', cleanup);
    };
    window.addEventListener('afterprint', cleanup);
    setTimeout(() => { if (!window.matchMedia('print').matches) cleanup(); }, 30000);
    window.print();
}

function exportQuotationPreview() {
    const data = _quotePreviewData;
    if (!data) return;
    const rows = [
        ['Quote #', data.orderId],
        ['Date', formatQuoteDate(data.orderDate)],
        ['Customer', data.customerName || ''],
        ['Customer ID', data.customerId != null ? data.customerId : 'N/A'],
        ['Validity', '15 Days'],
        [],
        ['Item', 'Qty', 'Price', 'Total'],
        ...(data.items || []).map(it => [it.name, it.qty, it.price, Number(it.price) * Number(it.qty)]),
        [],
        ['Subtotal', data.subtotal],
        ['Grand Total', data.totalAmount],
    ];
    const csv = rows.map(r => r.map(c => `"${String(c ?? '').replace(/"/g, '""')}"`).join(',')).join('\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8' });
    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = `quotation_${data.orderId}.csv`;
    a.click();
    URL.revokeObjectURL(a.href);
    toast(tr('saved_ok'), 'success');
}

let custDebtOnly = false;

function renderCustomers() {
    const q = (document.getElementById('cust-search')?.value || '').toLowerCase();
    const list = customers.filter(c => {
        if (custDebtOnly && !(Number(c.balance) > 0)) return false;
        return !q || [c.name, c.phone, c.email].some(x => (x || '').toLowerCase().includes(q));
    });
    document.getElementById('cust-body').innerHTML = list.length ? list.map(c => {
        const owed = Number(c.balance) || 0;
        return `<tr class="${owed > 0 ? 'row-has-debt' : ''}"><td>${escapeHtml(c.name)}</td><td>${escapeHtml(c.phone || '—')}</td><td>${escapeHtml(c.email || '—')}</td>
        <td class="${owed > 0 ? 'cell-debt' : ''}">${money(owed)}</td><td>${escapeHtml(c.type || '—')}</td>
        <td>${actionBtns(`data-edit-customer="${c.id}"`, `data-del-customer="${c.id}"`,
            `<button type="button" class="btn-icon" title="${tr('details')}" data-details-customer="${c.id}"><span class="material-symbols-rounded">visibility</span></button>`)}</td></tr>`;
    }).join('')
        : `<tr><td colspan="6" class="empty-state">${tr('empty_list')}</td></tr>`;
    document.querySelectorAll('[data-edit-customer]').forEach(btn => btn.onclick = () => openCustomerModal(Number(btn.dataset.editCustomer)));
    document.querySelectorAll('[data-del-customer]').forEach(btn => btn.onclick = () => deleteCustomer(Number(btn.dataset.delCustomer)));
    document.querySelectorAll('[data-details-customer]').forEach(btn => btn.onclick = () => openCustomerDetails(Number(btn.dataset.detailsCustomer)));
}

function renderSuppliers() {
    const q = (document.getElementById('supp-search')?.value || '').toLowerCase();
    const list = suppliers.filter(s => !q || [s.name, s.contact, s.phone].some(x => (x || '').toLowerCase().includes(q)));
    document.getElementById('supp-body').innerHTML = list.length ? list.map(s => `
        <tr><td>${escapeHtml(s.name)}</td><td>${escapeHtml(s.contact || '—')}</td>
        <td>${escapeHtml(s.phone || '—')}</td><td>${escapeHtml(s.email || '—')}</td>
        <td>${money(s.balance ?? 0)}</td>
        <td>${actionBtns(`data-edit-supplier="${s.id}"`, `data-del-supplier="${s.id}"`,
            `<button type="button" class="btn-icon" title="${tr('supplier_debt_products')}" data-purchases-supplier="${s.id}"><span class="material-symbols-rounded">inventory_2</span></button>` +
            `<button type="button" class="btn-icon" title="${tr('details')}" data-details-supplier="${s.id}"><span class="material-symbols-rounded">visibility</span></button>`)}</td></tr>`).join('')
        : `<tr><td colspan="6" class="empty-state">${tr('empty_list')}</td></tr>`;
    document.querySelectorAll('[data-edit-supplier]').forEach(btn => btn.onclick = () => openSupplierModal(Number(btn.dataset.editSupplier)));
    document.querySelectorAll('[data-del-supplier]').forEach(btn => btn.onclick = () => deleteSupplier(Number(btn.dataset.delSupplier)));
    document.querySelectorAll('[data-details-supplier]').forEach(btn => btn.onclick = () => openSupplierDetails(Number(btn.dataset.detailsSupplier)));
    document.querySelectorAll('[data-purchases-supplier]').forEach(btn => btn.onclick = () => openSupplierPurchases(Number(btn.dataset.purchasesSupplier)));
}

function syncExpenseDatePreset(preset) {
    const fromEl = document.getElementById('exp-filter-from');
    const toEl = document.getElementById('exp-filter-to');
    if (!fromEl || !toEl) return;
    const today = new Date();
    const setRange = (from, to) => {
        fromEl.value = from ? formatLocalDate(from) : '';
        toEl.value = to ? formatLocalDate(to) : '';
    };
    if (preset === 'all') {
        setRange(null, null);
    } else if (preset === 'today') {
        setRange(today, today);
    } else if (preset === 'this_week') {
        const start = new Date(today);
        const day = today.getDay();
        const diff = today.getDate() - day + (day === 0 ? -6 : 1);
        start.setDate(diff);
        setRange(start, today);
    } else if (preset === 'last_week') {
        const start = new Date(today);
        const day = today.getDay();
        const diff = today.getDate() - day - 6;
        start.setDate(diff);
        const end = new Date(start);
        end.setDate(start.getDate() + 6);
        setRange(start, end);
    } else if (preset === 'this_month') {
        setRange(new Date(today.getFullYear(), today.getMonth(), 1), today);
    }
}

function openMilkStatement(supplierId, from, to) {
    const supp = (suppliers || []).find(s => s.id === Number(supplierId)) || { name: tr('all_deliverers'), phone: '—' };
    const fromVal = from || document.getElementById('exp-filter-from')?.value || '';
    const toVal = to || document.getElementById('exp-filter-to')?.value || '';

    const items = (expenses || []).filter(e => {
        const isMilk = e.category === 'Raw Milk' || (e.liters != null && Number(e.liters) > 0);
        if (!isMilk) return false;
        if (supplierId && Number(e.supplierId) !== Number(supplierId)) return false;
        const d = String(e.expenseDate || '').slice(0, 10);
        if (fromVal && d < fromVal) return false;
        if (toVal && d > toVal) return false;
        return true;
    }).sort((a, b) => (a.expenseDate > b.expenseDate ? 1 : -1));

    let totalLiters = 0;
    let totalAmount = 0;
    let totalPaid = 0;
    let totalUnpaid = 0;

    items.forEach(it => {
        const lit = Number(it.liters) || 0;
        const amt = Number(it.amount) || 0;
        totalLiters += lit;
        totalAmount += amt;
        if (it.isPaid) totalPaid += amt;
        else totalUnpaid += amt;
    });

    const avgPrice = totalLiters > 0 ? (totalAmount / totalLiters) : 0;
    const printDate = formatLocalDate(new Date());
    const periodLabel = (fromVal && toVal) ? `${fromVal}  ➜  ${toVal}` : (fromVal ? `From ${fromVal}` : (toVal ? `Up to ${toVal}` : tr('preset_all')));

    const rowsHtml = items.length ? items.map((it, idx) => `
        <tr>
            <td>${idx + 1}</td>
            <td>${formatDate(it.expenseDate)}</td>
            <td>${escapeHtml(it.shift ? tr(it.shift === 'Morning' ? 'shift_morning' : it.shift === 'Evening' ? 'shift_evening' : it.shift) : '—')}</td>
            <td>${escapeHtml(it.milkType ? tr(it.milkType === 'Cow Milk' ? 'milk_cow' : it.milkType === 'Sheep Milk' ? 'milk_sheep' : it.milkType === 'Goat Milk' ? 'milk_goat' : it.milkType) : tr('milk_cow'))}</td>
            <td><strong>${(Number(it.liters) || 0).toLocaleString()} L</strong></td>
            <td>${money(it.unitPrice || 0)}</td>
            <td><strong>${money(it.amount || 0)}</strong></td>
            <td><span class="badge ${it.isPaid ? 'in-stock' : 'out-of-stock'}">${it.isPaid ? tr('paid') : tr('unpaid')}</span></td>
        </tr>
    `).join('') : `<tr><td colspan="8" class="empty-state">${tr('empty_list')}</td></tr>`;

    const html = `
        <div class="statement-sheet">
            <div class="statement-header">
                <div class="statement-logo-title">
                    <img src="nuricon_POS_web.png" alt="Al-Tayebat Logo">
                    <div>
                        <h2>Al-Tayebat Dairy • شركة الطيبات للألبان</h2>
                        <p>Dairy Farm & Processing • إدارة توريد الحليب الخام</p>
                    </div>
                </div>
                <div class="statement-badge-col">
                    <div class="statement-title-tag">${escapeHtml(tr('milk_weekly_statement'))}</div>
                    <div style="font-size:0.75rem; color:#64748b; margin-top: 4px;">${escapeHtml(tr('col_date'))}: ${printDate}</div>
                </div>
            </div>

            <div class="statement-meta-grid">
                <div class="statement-meta-item">
                    <label>${escapeHtml(tr('col_deliverer'))}</label>
                    <span>${escapeHtml(supp.name)}</span>
                </div>
                <div class="statement-meta-item">
                    <label>${escapeHtml(tr('col_phone'))}</label>
                    <span>${escapeHtml(supp.phone || '—')}</span>
                </div>
                <div class="statement-meta-item">
                    <label>${escapeHtml(tr('statement_period'))}</label>
                    <span>${escapeHtml(periodLabel)}</span>
                </div>
                <div class="statement-meta-item">
                    <label>${escapeHtml(tr('col_deliveries'))}</label>
                    <span>${items.length} ${escapeHtml(tr('col_items'))}</span>
                </div>
            </div>

            <table class="statement-table">
                <thead>
                    <tr>
                        <th style="width: 40px;">#</th>
                        <th>${escapeHtml(tr('col_date'))}</th>
                        <th>${escapeHtml(tr('delivery_shift'))}</th>
                        <th>${escapeHtml(tr('milk_type'))}</th>
                        <th>${escapeHtml(tr('milk_liters'))}</th>
                        <th>${escapeHtml(tr('price_per_liter'))}</th>
                        <th>${escapeHtml(tr('col_total'))}</th>
                        <th>${escapeHtml(tr('col_status'))}</th>
                    </tr>
                </thead>
                <tbody>
                    ${rowsHtml}
                </tbody>
                <tfoot>
                    <tr>
                        <td colspan="4" style="text-align: right; padding-right: 14px;">${escapeHtml(tr('subtotal'))} / ${escapeHtml(tr('total'))}:</td>
                        <td>${totalLiters.toLocaleString()} L</td>
                        <td>${money(avgPrice)}</td>
                        <td>${money(totalAmount)}</td>
                        <td>—</td>
                    </tr>
                </tfoot>
            </table>

            <div class="statement-summary-box">
                <table class="statement-summary-table">
                    <tr>
                        <td>${escapeHtml(tr('col_liters'))}:</td>
                        <td>${totalLiters.toLocaleString()} L</td>
                    </tr>
                    <tr>
                        <td>${escapeHtml(tr('col_amount'))}:</td>
                        <td>${money(totalAmount)}</td>
                    </tr>
                    <tr>
                        <td style="color:#16a34a;">${escapeHtml(tr('col_paid'))}:</td>
                        <td style="color:#16a34a;">${money(totalPaid)}</td>
                    </tr>
                    <tr class="due-highlight">
                        <td>${escapeHtml(tr('net_balance_due'))}:</td>
                        <td>${money(totalUnpaid)}</td>
                    </tr>
                </table>
            </div>

            <div class="statement-signatures">
                <div class="statement-sig-col">
                    <div class="statement-sig-line"></div>
                    <span>${escapeHtml(tr('deliverer_signature'))}</span>
                </div>
                <div class="statement-sig-col">
                    <div class="statement-sig-line"></div>
                    <span>${escapeHtml(tr('accountant_signature'))}</span>
                </div>
            </div>
        </div>
    `;

    document.getElementById('milk-statement-content').innerHTML = html;
    openModal('milk-statement-modal');
}

async function settleSelectedDeliverer() {
    const suppSel = document.getElementById('exp-filter-supplier');
    const suppId = suppSel ? Number(suppSel.value) : 0;
    if (!suppId) {
        toast(tr('select_deliverer_first'), 'warning');
        suppSel?.focus();
        return;
    }
    const supp = (suppliers || []).find(s => s.id === suppId);
    const suppName = supp ? supp.name : `Deliverer #${suppId}`;
    const fromVal = document.getElementById('exp-filter-from')?.value || '';
    const toVal = document.getElementById('exp-filter-to')?.value || '';

    const unpaidItems = (expenses || []).filter(e => {
        const isMilk = e.category === 'Raw Milk' || (e.liters != null && Number(e.liters) > 0);
        if (!isMilk || e.isPaid) return false;
        if (Number(e.supplierId) !== suppId) return false;
        const d = String(e.expenseDate || '').slice(0, 10);
        if (fromVal && d < fromVal) return false;
        if (toVal && d > toVal) return false;
        return true;
    });

    if (!unpaidItems.length) {
        toast(tr('no_unpaid_deliveries'), 'info');
        return;
    }

    const unpaidTotal = unpaidItems.reduce((acc, x) => acc + (Number(x.amount) || 0), 0);
    const confirmMsg = `${tr('confirm_settle_deliverer')}\n\n${suppName}\n${tr('col_deliveries')}: ${unpaidItems.length}\n${tr('total_due')}: ${money(unpaidTotal)}`;

    if (!await confirmDialog(confirmMsg, { danger: false, confirmText: tr('settle_deliverer') })) return;

    try {
        await api('/api/expenses/settle-supplier', {
            method: 'POST',
            body: JSON.stringify({
                supplierId: suppId,
                from: fromVal || null,
                to: toVal || null
            })
        });
        toast(tr('settle_ok'), 'success');
        await loadData();
        openMilkStatement(suppId, fromVal, toVal);
    } catch (err) {
        toast(err.message, 'error');
    }
}

function renderExpenses() {
    // Populate deliverers dropdown in filter if needed
    const suppSel = document.getElementById('exp-filter-supplier');
    if (suppSel && suppliers && suppliers.length) {
        const curVal = suppSel.value;
        const opts = [`<option value="">${escapeHtml(tr('all_deliverers'))}</option>`];
        suppliers.forEach(s => {
            opts.push(`<option value="${s.id}">${escapeHtml(s.name)}</option>`);
        });
        if (suppSel.dataset.lastCount !== String(suppliers.length)) {
            suppSel.innerHTML = opts.join('');
            suppSel.dataset.lastCount = String(suppliers.length);
            if (curVal && [...suppSel.options].some(o => o.value === curVal)) suppSel.value = curVal;
        }
    }

    const typeFilter = document.getElementById('exp-filter-type')?.value || 'all';
    const suppFilter = document.getElementById('exp-filter-supplier')?.value || '';
    const statusFilter = document.getElementById('exp-filter-status')?.value || 'all';
    const fromVal = document.getElementById('exp-filter-from')?.value || '';
    const toVal = document.getElementById('exp-filter-to')?.value || '';

    const filtered = (expenses || []).filter(e => {
        const isMilk = e.category === 'Raw Milk' || (e.liters != null && Number(e.liters) > 0);
        if (typeFilter === 'milk' && !isMilk) return false;
        if (typeFilter === 'general' && isMilk) return false;
        if (suppFilter && Number(e.supplierId) !== Number(suppFilter)) return false;
        if (statusFilter === 'paid' && !e.isPaid) return false;
        if (statusFilter === 'unpaid' && e.isPaid) return false;
        const d = String(e.expenseDate || '').slice(0, 10);
        if (fromVal && d < fromVal) return false;
        if (toVal && d > toVal) return false;
        return true;
    });

    // Compute stats
    let totalAll = 0, totalMilkLiters = 0, totalMilkCost = 0, totalMilkUnpaid = 0, totalGeneralCost = 0;
    filtered.forEach(e => {
        const amt = Number(e.amount) || 0;
        const isMilk = e.category === 'Raw Milk' || (e.liters != null && Number(e.liters) > 0);
        totalAll += amt;
        if (isMilk) {
            totalMilkCost += amt;
            totalMilkLiters += (Number(e.liters) || 0);
            if (!e.isPaid) totalMilkUnpaid += amt;
        } else {
            totalGeneralCost += amt;
        }
    });

    const avgPrice = totalMilkLiters > 0 ? (totalMilkCost / totalMilkLiters) : 0;

    // Render KPI Cards
    const statsGrid = document.getElementById('exp-stats');
    if (statsGrid) {
        const cards = [
            { label: tr('rep_expenses'), value: money(totalAll), sub: `${filtered.length} ${tr('col_items')}`, cls: '' },
            { label: tr('col_liters'), value: Number(totalMilkLiters).toLocaleString() + ' L', sub: `Avg: ${money(avgPrice)}/L`, cls: 'stat-milk' },
            { label: tr('milk_cost'), value: money(totalMilkCost), sub: tr('exp_filter_milk'), cls: 'stat-milk' },
            { label: tr('unpaid_milk_balance'), value: money(totalMilkUnpaid), sub: tr('due_to_deliverers'), cls: totalMilkUnpaid > 0 ? 'stat-due' : '' },
            { label: tr('rep_general_expenses'), value: money(totalGeneralCost), sub: tr('exp_filter_general'), cls: '' }
        ];
        statsGrid.innerHTML = cards.map(c => `
            <div class="stat-card ${c.cls || ''}">
                <div class="label">${escapeHtml(c.label)}</div>
                <div class="value">${c.value}</div>
                ${c.sub ? `<div class="stat-sub">${escapeHtml(c.sub)}</div>` : ''}
            </div>
        `).join('');
    }

    // Render table rows
    const tbody = document.getElementById('exp-body');
    if (tbody) {
        tbody.innerHTML = filtered.length ? filtered.map(e => {
            const isMilk = e.category === 'Raw Milk' || (e.liters != null && Number(e.liters) > 0);
            let delivererDetails = '';
            if (isMilk) {
                delivererDetails = `
                    <strong>${escapeHtml(e.supplierName || tr('unassigned'))}</strong>
                    <br>
                    <small style="color:var(--text-muted); font-size:0.78rem;">
                        ${e.liters ? `${(Number(e.liters) || 0).toLocaleString()} L @ ${money(e.unitPrice || 0)}/L` : ''}
                        ${e.milkType ? ` • ${escapeHtml(tr(e.milkType === 'Cow Milk' ? 'milk_cow' : e.milkType === 'Sheep Milk' ? 'milk_sheep' : e.milkType === 'Goat Milk' ? 'milk_goat' : e.milkType))}` : ''}
                        ${e.shift ? ` • ${escapeHtml(tr(e.shift === 'Morning' ? 'shift_morning' : e.shift === 'Evening' ? 'shift_evening' : e.shift))}` : ''}
                    </small>`;
            } else if (e.supplierName) {
                delivererDetails = `<strong>${escapeHtml(e.supplierName)}</strong>`;
            } else {
                delivererDetails = `<span style="color:var(--text-muted); font-size:0.85rem;">—</span>`;
            }

            return `
                <tr>
                    <td>${formatDate(e.expenseDate)}</td>
                    <td><span class="${isMilk ? 'e-milk-badge' : 'badge badge-default'}">${escapeHtml(e.category)}</span></td>
                    <td>${delivererDetails}</td>
                    <td><strong>${money(e.amount)}</strong></td>
                    <td>${escapeHtml(e.description || '—')}</td>
                    <td>
                        <span class="badge ${e.isPaid ? 'in-stock' : 'out-of-stock'}" 
                              role="button" 
                              tabindex="0"
                              data-toggle-expense="${e.expenseId}" 
                              style="cursor:pointer; user-select:none; display:inline-flex; align-items:center; gap:4px;"
                              title="${e.isPaid ? tr('mark_unpaid') : tr('mark_paid')}">
                            <span class="material-symbols-rounded" style="font-size:14px;">${e.isPaid ? 'check_circle' : 'pending'}</span>
                            ${e.isPaid ? tr('paid') : tr('unpaid')}
                        </span>
                    </td>
                    <td>
                        <div class="table-actions">
                            ${e.isPaid ? `
                                <button type="button" class="btn btn-secondary btn-sm" data-unpay="${e.expenseId}" title="${tr('mark_unpaid')}">
                                    <span class="material-symbols-rounded" style="font-size:15px; vertical-align:middle; margin-inline-end:2px;">undo</span>
                                    <span>${tr('mark_unpaid')}</span>
                                </button>
                            ` : `
                                <button type="button" class="btn btn-secondary btn-sm" data-pay="${e.expenseId}" title="${tr('mark_paid')}">
                                    <span class="material-symbols-rounded" style="font-size:15px; vertical-align:middle; margin-inline-end:2px;">check_circle</span>
                                    <span>${tr('mark_paid')}</span>
                                </button>
                            `}
                            ${isMilk && e.supplierId ? `<button type="button" class="btn btn-secondary btn-sm" title="${tr('milk_weekly_statement')}" data-statement-supplier="${e.supplierId}"><span class="material-symbols-rounded" style="font-size:16px;">receipt_long</span></button>` : ''}
                            <button type="button" class="btn-icon btn-icon-danger" title="${tr('delete')}" data-del-expense="${e.expenseId}"><span class="material-symbols-rounded">delete</span></button>
                        </div>
                    </td>
                </tr>
            `;
        }).join('') : `<tr><td colspan="7" class="empty-state">${tr('empty_list')}</td></tr>`;

        tbody.querySelectorAll('[data-pay]').forEach(btn => {
            btn.onclick = async () => {
                try {
                    await api('/api/expenses/' + btn.dataset.pay + '/pay', { method: 'POST' });
                    toast(tr('saved_ok'), 'success');
                    await loadData();
                } catch (e) { toast(e.message, 'error'); }
            };
        });

        tbody.querySelectorAll('[data-unpay]').forEach(btn => {
            btn.onclick = async () => {
                try {
                    await api('/api/expenses/' + btn.dataset.unpay + '/unpay', { method: 'POST' });
                    toast(tr('saved_ok'), 'success');
                    await loadData();
                } catch (e) { toast(e.message, 'error'); }
            };
        });

        tbody.querySelectorAll('[data-toggle-expense]').forEach(el => {
            el.onclick = async () => {
                try {
                    await api('/api/expenses/' + el.dataset.toggleExpense + '/toggle-paid', { method: 'POST' });
                    toast(tr('saved_ok'), 'success');
                    await loadData();
                } catch (e) { toast(e.message, 'error'); }
            };
        });

        tbody.querySelectorAll('[data-statement-supplier]').forEach(btn => {
            btn.onclick = () => {
                openMilkStatement(Number(btn.dataset.statementSupplier));
            };
        });

        tbody.querySelectorAll('[data-del-expense]').forEach(btn => {
            btn.onclick = () => deleteExpense(Number(btn.dataset.delExpense));
        });
    }
}

function renderCurrencies() {
    document.getElementById('cur-body').innerHTML = currencies.length ? currencies.map(c => `
        <tr><td>${escapeHtml(c.code)}</td><td>${escapeHtml(c.name)}</td>
        <td>${escapeHtml(c.symbol)}</td><td>${Number(c.rate).toFixed(4)}</td>
        <td>${c.code === 'USD' ? '—' : `<button type="button" class="btn-icon btn-icon-danger" title="${tr('delete')}" data-del-cur="${escapeHtml(c.code)}"><span class="material-symbols-rounded">delete</span></button>`}</td></tr>`).join('')
        : `<tr><td colspan="5" class="empty-state">${tr('empty_list')}</td></tr>`;
    document.querySelectorAll('[data-del-cur]').forEach(btn => btn.onclick = () => deleteCurrency(btn.dataset.delCur));
}

function filteredBarcodeItems() {
    const q = (document.getElementById('bar-search')?.value || '').toLowerCase();
    return barcodeItems.filter(b => !q || [b.name, b.sku, b.barcode].some(x => (x || '').toLowerCase().includes(q)));
}

function barcodeItemKey(b) {
    return String(b.id ?? b.sku ?? b.barcode ?? b.name);
}

function barcodeItemCode(b) {
    return String(b.barcode || b.sku || b.id || '').trim();
}

function drawJsBarcode(svg, code, opts = {}) {
    if (!window.JsBarcode || !svg || !code) return false;
    try {
        JsBarcode(svg, code, {
            format: 'CODE128',
            width: opts.width ?? 1.6,
            height: opts.height ?? 48,
            displayValue: opts.displayValue !== false,
            fontSize: opts.fontSize ?? 12,
            margin: opts.margin ?? 4,
            background: opts.background ?? '#ffffff',
            lineColor: '#000000',
            textMargin: 4,
            ...opts
        });
        return true;
    } catch (e) {
        console.warn('JsBarcode failed for', code, e);
        return false;
    }
}

function renderBarcodes() {
    const list = filteredBarcodeItems();
    const grid = document.getElementById('barcodes-grid');
    if (!grid) return;
    const visibleKeys = list.slice(0, 120).map(barcodeItemKey);
    // Drop selections that are no longer in the filtered set
    [...barcodeSelected].forEach(k => {
        if (!list.some(b => barcodeItemKey(b) === k)) barcodeSelected.delete(k);
    });
    grid.innerHTML = list.length ? list.slice(0, 120).map(b => {
        const key = barcodeItemKey(b);
        const code = barcodeItemCode(b) || '—';
        const checked = barcodeSelected.has(key) ? 'checked' : '';
        return `<label class="barcode-card${checked ? ' selected' : ''}">
            <input type="checkbox" class="bc-check" data-bar-id="${escapeHtml(key)}" ${checked}>
            <div class="bc-wrap"><svg class="bc-svg" data-code="${escapeHtml(code)}"></svg></div>
            <h3 title="${escapeHtml(b.name || '')}">${escapeHtml(b.name || '—')}</h3>
            <p>${escapeHtml(code)}</p>
            <div class="price">${money(b.price)}</div>
        </label>`;
    }).join('') : `<div class="empty-state"><span class="material-symbols-rounded">qr_code_2</span><p>${tr('empty_list')}</p></div>`;

    grid.querySelectorAll('.bc-svg').forEach(svg => {
        const code = svg.dataset.code || '';
        if (!code || code === '—') return;
        drawJsBarcode(svg, code, { width: 1.5, height: 42, fontSize: 11, margin: 2, background: 'transparent' });
        svg.removeAttribute('width');
        svg.removeAttribute('height');
        svg.setAttribute('preserveAspectRatio', 'xMidYMid meet');
        svg.style.width = '100%';
        svg.style.height = '100%';
    });

    grid.querySelectorAll('.bc-check').forEach(cb => {
        cb.addEventListener('change', () => {
            const id = cb.dataset.barId;
            if (cb.checked) barcodeSelected.add(id);
            else barcodeSelected.delete(id);
            cb.closest('.barcode-card')?.classList.toggle('selected', cb.checked);
            syncBarcodeSelectAll();
        });
    });

    const selAll = document.getElementById('bar-select-all');
    if (selAll) {
        const allVisibleSelected = visibleKeys.length > 0 && visibleKeys.every(k => barcodeSelected.has(k));
        selAll.checked = allVisibleSelected;
        selAll.indeterminate = !allVisibleSelected && visibleKeys.some(k => barcodeSelected.has(k));
    }
}

function syncBarcodeSelectAll() {
    const list = filteredBarcodeItems().slice(0, 120);
    const keys = list.map(barcodeItemKey);
    const selAll = document.getElementById('bar-select-all');
    if (!selAll) return;
    const allOn = keys.length > 0 && keys.every(k => barcodeSelected.has(k));
    selAll.checked = allOn;
    selAll.indeterminate = !allOn && keys.some(k => barcodeSelected.has(k));
}

function estimateBarcodeSheets(labelCount, landscape) {
    const size = getLiveLabelSize();
    if (size.paperMode === 'roll') return Math.max(1, labelCount);
    const pageW = landscape ? size.pageHeightMm : size.pageWidthMm;
    const pageH = landscape ? size.pageWidthMm : size.pageHeightMm;
    const margin = (size.marginMm || 0) * 2;
    const gap = size.labelGapMm || 5;
    const cellW = size.labelWidthMm + gap;
    const cellH = size.labelHeightMm + gap;
    const cols = Math.max(1, Math.floor((pageW - margin) / cellW));
    const rows = Math.max(1, Math.floor((pageH - margin) / cellH));
    const perPage = cols * rows;
    return Math.max(1, Math.ceil(Math.max(1, labelCount) / perPage));
}

function updateBarcodePrintSheetMeta(count) {
    const el = document.getElementById('bpd-sheet-count');
    if (!el) return;
    const landscape = document.querySelector('input[name="bpd-layout"]:checked')?.value === 'landscape';
    const sheets = estimateBarcodeSheets(count, landscape);
    el.textContent = sheets === 1
        ? tr('print_sheet_one')
        : tr('print_sheet_many').replace('{0}', String(sheets));
}

function applyBarcodePrintLayoutPreview() {
    const paper = document.getElementById('bpd-paper');
    if (!paper) return;
    const landscape = document.querySelector('input[name="bpd-layout"]:checked')?.value === 'landscape';
    paper.classList.toggle('is-landscape', landscape);
    const size = getLiveLabelSize();
    paper.classList.toggle('is-roll', size.paperMode === 'roll');
    paper.style.setProperty('--label-w', `${size.labelWidthMm}mm`);
    paper.style.setProperty('--label-h', `${size.labelHeightMm}mm`);
    paper.style.setProperty('--label-gap', `${size.labelGapMm}mm`);
    if (size.paperMode === 'roll') {
        paper.style.width = '';
        paper.style.aspectRatio = '';
    } else {
        const pw = landscape ? size.pageHeightMm : size.pageWidthMm;
        const ph = landscape ? size.pageWidthMm : size.pageHeightMm;
        paper.style.width = `min(100%, ${Math.round(pw * 2.2)}px)`;
        paper.style.aspectRatio = `${pw} / ${ph}`;
    }
    const sheet = document.getElementById('barcode-print-sheet');
    if (sheet) {
        sheet.style.gridTemplateColumns = size.paperMode === 'roll' ? '1fr' : '';
    }
    toggleSheetPageRows();
    const selected = barcodeItems.filter(b => barcodeSelected.has(barcodeItemKey(b)));
    updateBarcodePrintSheetMeta(selected.length);
}

function fillPrinterSelect(selId, printers, preferredPrinter) {
    const sel = document.getElementById(selId);
    if (!sel) return;
    const list = Array.isArray(printers) ? printers.filter(Boolean) : [];
    if (!list.length) {
        sel.innerHTML = `<option value="">${escapeHtml(tr('print_system_printer'))}</option>`;
        return;
    }
    const preferred = preferredPrinter && list.includes(preferredPrinter)
        ? preferredPrinter
        : list[0];
    sel.innerHTML = list.map(p =>
        `<option value="${escapeHtml(p)}"${p === preferred ? ' selected' : ''}>${escapeHtml(p)}</option>`
    ).join('');
}

function fillBarcodePrinterSelect(printers, defaultPrinter) {
    const labelPref = printSettings.labelPrinter || defaultPrinter;
    const receiptPref = printSettings.receiptPrinter || defaultPrinter;
    fillPrinterSelect('bpd-printer', printers, labelPref);
    fillPrinterSelect('ppd-printer', printers, receiptPref);
    fillPrinterSelect('ps-label-printer', printers, labelPref);
    fillPrinterSelect('ps-receipt-printer', printers, receiptPref);
    fillBarcodePrintSizeControls(document.getElementById('bpd-printer')?.value);
    fillPosPrintSizeControls(document.getElementById('ppd-printer')?.value);
    applyBarcodePrintLayoutPreview();
    applyPosPrintLayoutPreview();
}

function requestHostPrinters() {
    try {
        if (window.chrome?.webview?.postMessage)
            window.chrome.webview.postMessage(JSON.stringify({ action: 'listPrinters' }));
        else
            fillBarcodePrinterSelect([], '');
    } catch {
        fillBarcodePrinterSelect([], '');
    }
}

function openBarcodePrintPreview() {
    const selected = barcodeItems.filter(b => barcodeSelected.has(barcodeItemKey(b)));
    if (!selected.length) {
        toast(tr('select_barcodes_first'), 'error');
        return;
    }
    if (!window.JsBarcode) {
        toast('Barcode library not loaded', 'error');
        return;
    }
    const sheet = document.getElementById('barcode-print-sheet');
    if (!sheet) return;
    sheet.innerHTML = selected.map(b => {
        const code = barcodeItemCode(b);
        return `<div class="bc-label">
            <div class="bc-label-name">${escapeHtml(b.name || '')}</div>
            <svg class="bc-print-svg" data-code="${escapeHtml(code)}"></svg>
            <div class="bc-label-code">${escapeHtml(code)}</div>
            <div class="bc-label-price">${money(b.price)}</div>
        </div>`;
    }).join('');

    const title = document.getElementById('barcode-preview-title');
    if (title) title.textContent = tr('print');
    fillBarcodePrintSizeControls();
    updateBarcodePrintSheetMeta(selected.length);
    applyBarcodePrintLayoutPreview();
    requestHostPrinters();
    openModal('barcode-preview-modal');

    setTimeout(() => {
        sheet.querySelectorAll('.bc-print-svg').forEach(svg => {
            const code = svg.dataset.code || '';
            if (!code) {
                const miss = document.createElement('div');
                miss.className = 'bc-label-missing';
                miss.textContent = '—';
                svg.replaceWith(miss);
                return;
            }
            const ok = drawJsBarcode(svg, code, {
                width: 2,
                height: 48,
                fontSize: 12,
                margin: 4,
                displayValue: false,
                background: '#ffffff'
            });
            if (!ok) {
                const miss = document.createElement('div');
                miss.className = 'bc-label-missing';
                miss.textContent = code;
                svg.replaceWith(miss);
            }
        });
    }, 40);
}

function getBarcodePrintOptions() {
    const landscape = document.querySelector('input[name="bpd-layout"]:checked')?.value === 'landscape';
    const pagesMode = document.querySelector('input[name="bpd-pages"]:checked')?.value || 'all';
    const pageRange = pagesMode === 'custom'
        ? (document.getElementById('bpd-page-range')?.value || '').trim()
        : 'all';
    const copies = Math.max(1, Math.min(99, parseInt(document.getElementById('bpd-copies')?.value, 10) || 1));
    const color = (document.getElementById('bpd-color')?.value || 'color') !== 'bw';
    const printerName = document.getElementById('bpd-printer')?.value || '';
    const size = getLiveLabelSize();
    return {
        landscape,
        pageRange,
        copies,
        color,
        printerName,
        labelWidthMm: size.labelWidthMm,
        labelHeightMm: size.labelHeightMm,
        labelGapMm: size.labelGapMm,
        marginMm: size.marginMm,
        paperMode: size.paperMode,
        pageWidthMm: size.pageWidthMm,
        pageHeightMm: size.pageHeightMm
    };
}

function printBarcodePreview() {
    const selected = barcodeItems.filter(b => barcodeSelected.has(barcodeItemKey(b)));
    if (!selected.length) {
        toast(tr('select_barcodes_first'), 'error');
        return;
    }

    const opts = getBarcodePrintOptions();
    const items = selected.map(b => ({
        name: b.name || '',
        sku: barcodeItemCode(b),
        barcode: barcodeItemCode(b),
        price: Number(b.price) || 0,
        quantity: 1
    }));

    // Desktop host: print via WinForms (app-styled dialog already shown)
    if (window.chrome?.webview?.postMessage) {
        try {
            window.chrome.webview.postMessage(JSON.stringify({
                action: 'printBarcodes',
                items,
                printerName: opts.printerName,
                copies: opts.copies,
                landscape: opts.landscape,
                color: opts.color,
                pageRange: opts.pageRange || 'all',
                labelWidthMm: opts.labelWidthMm,
                labelHeightMm: opts.labelHeightMm,
                labelGapMm: opts.labelGapMm,
                marginMm: opts.marginMm,
                paperMode: opts.paperMode,
                pageWidthMm: opts.pageWidthMm,
                pageHeightMm: opts.pageHeightMm
            }));
            closeModal('barcode-preview-modal');
            return;
        } catch (e) {
            toast(tr('print_failed'), 'error');
            return;
        }
    }

    // Browser fallback: system print dialog
    let sheet = document.getElementById('barcode-print-sheet');
    if (!sheet || !sheet.children.length) {
        openBarcodePrintPreview();
        sheet = document.getElementById('barcode-print-sheet');
        if (!sheet?.children.length) return;
    }

    let root = document.getElementById('barcode-print-root');
    if (!root) {
        root = document.createElement('div');
        root.id = 'barcode-print-root';
        root.setAttribute('aria-hidden', 'true');
        document.body.appendChild(root);
    }
    root.innerHTML = `<div class="barcode-print-sheet barcode-print-sheet--print">${sheet.innerHTML}</div>`;

    root.querySelectorAll('.bc-print-svg').forEach(svg => {
        const code = svg.getAttribute('data-code') || svg.dataset.code || '';
        if (!code) return;
        drawJsBarcode(svg, code, {
            width: 2.4,
            height: 64,
            fontSize: 14,
            margin: 4,
            displayValue: false,
            background: '#ffffff'
        });
    });

    const live = getLiveLabelSize();
    document.documentElement.style.setProperty('--label-w', `${live.labelWidthMm}mm`);
    document.documentElement.style.setProperty('--label-h', `${live.labelHeightMm}mm`);
    document.documentElement.style.setProperty('--label-gap', `${live.labelGapMm}mm`);

    document.body.classList.add('printing-barcodes');
    const cleanup = () => {
        document.body.classList.remove('printing-barcodes');
        root.innerHTML = '';
        window.removeEventListener('afterprint', cleanup);
    };
    window.addEventListener('afterprint', cleanup);
    setTimeout(() => { if (!window.matchMedia('print').matches) cleanup(); }, 60000);
    setTimeout(() => { window.print(); }, 120);
}

function isProtectedSuperAdmin(u) {
    const name = String(u?.username || '').trim().toLowerCase();
    return name === 'softio.admin';
}

function renderUsers() {
    document.getElementById('users-body').innerHTML = users.length ? users.map(u => {
        const protectedAdmin = isProtectedSuperAdmin(u);
        const actions = u.id == null ? '—'
            : protectedAdmin
                ? `<span class="muted" title="${tr('lic_valid')}">Softio</span>`
                : actionBtns(`data-edit-user="${u.id}"`, `data-del-user="${u.id}"`);
        return `<tr><td>${escapeHtml(u.username)}</td><td>${escapeHtml(u.fullName || '—')}</td><td>${escapeHtml(formatRole(u.role))}</td>
        <td>${actions}</td></tr>`;
    }).join('')
        : `<tr><td colspan="4" class="empty-state">${tr('empty_list')}</td></tr>`;
    document.querySelectorAll('[data-edit-user]').forEach(btn => btn.onclick = () => openUserModal(Number(btn.dataset.editUser)));
    document.querySelectorAll('[data-del-user]').forEach(btn => btn.onclick = () => deleteUser(Number(btn.dataset.delUser)));
}

function renderLicense() {
    const el = document.getElementById('license-info');
    if (!el) return;
    const lic = window._license;
    if (!lic || (lic.isValid === false && !lic.licenseType)) {
        el.innerHTML = `<div class="empty-state" style="padding:0.5rem 0;">${tr('lic_invalid')}</div>`;
        return;
    }
    const statusClass = lic.isValid ? (lic.isTrial ? 'trial' : 'valid') : 'invalid';
    const statusText = lic.isValid ? (lic.isTrial ? tr('lic_trial') : tr('lic_valid')) : tr('lic_invalid');
    el.innerHTML = `<dl class="settings-kv">
        <dt>${escapeHtml(tr('lic_type'))}</dt>
        <dd>${escapeHtml(lic.licenseType || '—')}</dd>
        <dt>${escapeHtml(tr('lic_customer'))}</dt>
        <dd>${escapeHtml(lic.customerName || '—')}</dd>
        <dt>${escapeHtml(tr('lic_expires'))}</dt>
        <dd>${formatDate(lic.expirationDate)}</dd>
        <dt>${escapeHtml(tr('lic_days'))}</dt>
        <dd>${lic.daysRemaining ?? '—'}</dd>
        <dt>${escapeHtml(tr('lic_machine'))}</dt>
        <dd>${escapeHtml(lic.machineName || '—')}</dd>
        <dt>${escapeHtml(tr('lic_valid'))}</dt>
        <dd><span class="settings-status-pill ${statusClass}">${escapeHtml(statusText)}</span>${lic.keyMasked ? ' · ' + escapeHtml(lic.keyMasked) : ''}</dd>
    </dl>`;
}

function updateSettingsLangLabel() {
    const el = document.getElementById('settings-lang-label');
    if (el) el.textContent = lang === 'ar' ? 'العربية' : 'English';
}

function toggleLanguage() {
    lang = lang === 'en' ? 'ar' : 'en';
    localStorage.setItem('generic_lang', lang);
    applyI18n();
    renderAll();
    updateSettingsLangLabel();
    refreshNotifications().then(list => {
        if (document.getElementById('notif-modal')?.classList.contains('active'))
            renderNotificationList(list);
    });
}

async function openLicenseModal() {
    const isWall = document.getElementById('license-modal')?.classList.contains('license-wall-blocking');
    const err = document.getElementById('license-activate-error');
    if (err && !isWall) { err.textContent = ''; err.classList.remove('visible'); }
    const key = document.getElementById('lic-key');
    if (key) key.value = '';
    let hw = window._license?.hardwareId;
    if (!hw) {
        try {
            const r = await api('/api/license');
            hw = r.hardwareId;
            window._license = { ...(window._license || {}), ...r };
        } catch { hw = '—'; }
    }
    const hwid = document.getElementById('lic-hwid');
    if (hwid) hwid.textContent = hw || '—';

    const lic = window._license;
    const isExpired = !lic || lic.isValid === false || (lic.daysRemaining !== undefined && lic.daysRemaining <= 0);

    const startTrialBtn = document.getElementById('btn-start-trial');
    if (startTrialBtn) {
        const canStart = lic && lic.canStartTrial === true && !lic.isTrial && !isExpired;
        startTrialBtn.style.display = canStart ? '' : 'none';
    }

    const cancelBtn = document.getElementById('btn-lic-cancel') || document.querySelector('#license-modal [data-close="license-modal"]');
    if (cancelBtn) cancelBtn.style.display = isExpired ? 'none' : '';

    const closeAppBtn = document.getElementById('btn-lic-close-app');
    if (closeAppBtn) {
        closeAppBtn.style.display = isExpired ? '' : 'none';
        closeAppBtn.onclick = () => postHost('close');
    }

    const closeBtn = document.querySelector('#license-modal .close-btn');
    if (closeBtn) {
        closeBtn.style.display = isExpired ? 'none' : '';
    }

    openModal('license-modal');
}

function updateBadges() {
    const low = products.filter(p => p.stock <= (p.minStock || 0)).length;
    const badge = document.getElementById('badge-inventory');
    if (low > 0) { badge.style.display = ''; badge.textContent = low; }
    else badge.style.display = 'none';
}

function updateReturnRefundTotal(prefix) {
    const root = prefix === 'pos' ? document.getElementById('pos-return-items') : document.getElementById('return-items');
    const totalEl = document.getElementById(prefix === 'pos' ? 'pos-return-refund-total' : 'return-refund-total');
    const cache = prefix === 'pos' ? posReturnItemsCache : returnItemsCache;
    const qtySel = prefix === 'pos' ? '.pos-ret-qty' : '.ret-qty';
    if (!root || !totalEl) return;
    let total = 0;
    root.querySelectorAll(qtySel).forEach(inp => {
        const i = Number(inp.dataset.i);
        const qty = Math.max(0, Number(inp.value) || 0);
        const it = cache[i];
        if (!it) return;
        total += (Number(it.price) || 0) * qty;
        const lineEl = inp.closest('.return-line')?.querySelector('.ret-line-total');
        if (lineEl) lineEl.textContent = money((Number(it.price) || 0) * qty);
    });
    totalEl.textContent = money(total);
}

function renderReturnableItems(containerId, items, qtyClass = 'ret-qty') {
    const el = document.getElementById(containerId);
    if (!el) return;
    if (!items?.length) {
        el.innerHTML = `<div class="empty-state">${tr('empty_list')}</div>`;
        return;
    }
    el.innerHTML = items.map((it, i) => {
        const rem = Number(it.remainingQty ?? it.qty) || 0;
        const sold = Number(it.soldQty ?? it.qty) || 0;
        const price = Number(it.price) || 0;
        const disabled = rem <= 0 ? 'disabled' : '';
        return `<div class="return-line ${rem <= 0 ? 'is-done' : ''}">
            <div class="return-line-main">
                <strong>${escapeHtml(it.name)}</strong>
                <span class="return-line-price">${tr('unit_price')}: <b>${money(price)}</b></span>
            </div>
            <div class="return-line-meta">
                <span>${tr('sold_qty')}: ${sold}</span>
                <span>${tr('can_return')}: ${rem}</span>
            </div>
            <div class="return-line-qty">
                <label>${tr('return_qty')}</label>
                <input type="number" class="form-control ${qtyClass}" data-i="${i}" min="0" max="${rem}" value="${rem > 0 ? rem : 0}" ${disabled}>
                <span class="ret-line-total">${money(price * (rem > 0 ? rem : 0))}</span>
            </div>
        </div>`;
    }).join('');
    el.querySelectorAll(`.${qtyClass}`).forEach(inp => {
        inp.oninput = () => {
            const max = Number(inp.max) || 0;
            let v = Number(inp.value);
            if (v > max) inp.value = max;
            if (v < 0) inp.value = 0;
            updateReturnRefundTotal(qtyClass.startsWith('pos') ? 'pos' : 'sales');
        };
    });
}

function setPosReturnStep(step) {
    ['customer', 'sales', 'items'].forEach(s => {
        const el = document.getElementById(`pos-return-step-${s}`);
        if (el) el.hidden = s !== step;
    });
}

function openPosReturnModal() {
    posReturnOrderId = null;
    posReturnItemsCache = [];
    const sel = document.getElementById('pos-return-customer');
    if (sel) {
        const cur = document.getElementById('pos-customer')?.value || '';
        sel.innerHTML = `<option value="">${escapeHtml(tr('walk_in_customer'))}</option>` +
            customers.map(c => `<option value="${c.id}">${escapeHtml(c.name)}</option>`).join('');
        if (cur && [...sel.options].some(o => o.value === cur)) sel.value = cur;
    }
    const orderInp = document.getElementById('pos-return-order-id');
    if (orderInp) orderInp.value = '';
    const reason = document.getElementById('pos-return-reason');
    if (reason) reason.value = '';
    setPosReturnStep('customer');
    openModal('pos-return-modal');
}

async function loadPosReturnSales() {
    const customerId = Number(document.getElementById('pos-return-customer')?.value || 0);
    if (!customerId) return toast(tr('select_customer_or_order'), 'error');
    try {
        const list = await api('/api/customers/' + customerId + '/sales');
        const box = document.getElementById('pos-return-sales-list');
        if (!box) return;
        if (!list?.length) {
            box.innerHTML = `<div class="empty-state">${tr('no_sales_for_customer')}</div>`;
        } else {
            box.innerHTML = list.map(o => {
                const paid = String(o.paymentStatus || 'Paid').toLowerCase() === 'paid';
                return `<button type="button" class="pos-return-sale-card" data-order="${o.orderId}">
                    <div class="pos-return-sale-top">
                        <strong>#${o.orderId}</strong>
                        <span class="badge ${paid ? 'in-stock' : 'low-stock'}">${paid ? tr('paid') : tr('unpaid')}</span>
                    </div>
                    <div class="pos-return-sale-sub">${formatDate(o.date)}</div>
                    <div class="pos-return-sale-sub">${o.itemCount || 0} ${tr('col_items').toLowerCase()} · ${money(o.total)}</div>
                </button>`;
            }).join('');
            box.querySelectorAll('[data-order]').forEach(btn => {
                btn.onclick = () => openPosReturnOrder(Number(btn.dataset.order));
            });
        }
        setPosReturnStep('sales');
    } catch (e) { toast(e.message, 'error'); }
}

async function openPosReturnOrder(orderId) {
    try {
        const data = await api('/api/orders/' + orderId + '/returnable');
        posReturnOrderId = data.orderId;
        posReturnItemsCache = data.items || [];
        const meta = document.getElementById('pos-return-order-meta');
        if (meta) {
            meta.hidden = false;
            meta.innerHTML = `<strong>#${data.orderId}</strong> · ${escapeHtml(data.customerName || '')} · ${formatDate(data.date)} · ${money(data.total)}`;
        }
        renderReturnableItems('pos-return-items', posReturnItemsCache, 'pos-ret-qty');
        // Bind with correct prefix after render - fix the oninput to use pos
        document.querySelectorAll('#pos-return-items .pos-ret-qty').forEach(inp => {
            inp.oninput = () => {
                const max = Number(inp.max) || 0;
                let v = Number(inp.value);
                if (v > max) inp.value = max;
                if (v < 0) inp.value = 0;
                updateReturnRefundTotal('pos');
            };
        });
        updateReturnRefundTotal('pos');
        if (!posReturnItemsCache.some(x => (x.remainingQty || 0) > 0)) {
            toast(tr('already_fully_returned'), 'error');
        }
        setPosReturnStep('items');
    } catch (e) { toast(e.message, 'error'); }
}

async function submitPosReturn() {
    const items = [];
    let count = 0;
    let refund = 0;
    document.querySelectorAll('#pos-return-items .pos-ret-qty').forEach(inp => {
        const i = Number(inp.dataset.i);
        const qty = Number(inp.value);
        const it = posReturnItemsCache[i];
        if (qty > 0 && it) {
            const amt = (Number(it.price) || 0) * qty;
            items.push({ partId: it.partId, qty, refundAmount: amt });
            count += qty;
            refund += amt;
        }
    });
    if (!items.length) return toast(tr('empty_cart_hint'), 'error');
    try {
        await api('/api/return-item', {
            method: 'POST',
            body: JSON.stringify({
                orderId: posReturnOrderId,
                reason: document.getElementById('pos-return-reason')?.value.trim() || 'POS return',
                items
            })
        });
        closeModal('pos-return-modal');
        toast(tr('return_ok_detail').replace('{0}', String(count)).replace('{1}', money(refund)), 'success');
        await loadData();
        renderPosStats();
    } catch (e) { toast(e.message, 'error'); }
}

async function openReturn(orderId) {
    returnOrderId = orderId;
    try {
        const data = await api(`/api/orders/${orderId}/returnable`);
        returnItemsCache = data.items || [];
        const meta = document.getElementById('return-order-meta');
        if (meta) {
            meta.hidden = false;
            meta.innerHTML = `<strong>#${data.orderId}</strong> · ${escapeHtml(data.customerName || '')} · ${formatDate(data.date)} · ${money(data.total)}`;
        }
        renderReturnableItems('return-items', returnItemsCache, 'ret-qty');
        document.querySelectorAll('#return-items .ret-qty').forEach(inp => {
            inp.oninput = () => {
                const max = Number(inp.max) || 0;
                let v = Number(inp.value);
                if (v > max) inp.value = max;
                if (v < 0) inp.value = 0;
                updateReturnRefundTotal('sales');
            };
        });
        updateReturnRefundTotal('sales');
        document.getElementById('return-reason').value = '';
        openModal('return-modal');
    } catch (e) { toast(e.message, 'error'); }
}

function openModal(id) { document.getElementById(id).classList.add('active'); }
function closeModal(id) {
    const wasActive = document.getElementById(id)?.classList.contains('active');
    document.getElementById(id).classList.remove('active');
    if (id === 'product-modal' && wasActive && fsImportQueue?.awaitingSave)
        onFsImportProductCancelled();
}

function confirmDialog(message, options = {}) {
    const {
        title = tr('confirm_title'),
        confirmText = tr('delete'),
        cancelText = tr('cancel'),
        danger = true,
        hideCancel = false,
    } = options;
    const overlay = document.getElementById('confirm-modal');
    const titleEl = document.getElementById('confirm-title');
    const messageEl = document.getElementById('confirm-message');
    const okBtn = document.getElementById('confirm-ok');
    const cancelBtn = document.getElementById('confirm-cancel');
    if (!overlay || !titleEl || !messageEl || !okBtn || !cancelBtn) return Promise.resolve(false);

    titleEl.textContent = title;
    messageEl.textContent = message;
    okBtn.textContent = confirmText;
    cancelBtn.textContent = cancelText;
    okBtn.className = danger ? 'btn btn-danger' : 'btn btn-primary';
    cancelBtn.hidden = !!hideCancel;

    return new Promise(resolve => {
        const finish = (result) => {
            okBtn.onclick = null;
            cancelBtn.onclick = null;
            overlay.onclick = null;
            document.removeEventListener('keydown', onKey);
            cancelBtn.hidden = false;
            closeModal('confirm-modal');
            resolve(result);
        };
        const onKey = (e) => {
            if (e.key === 'Escape') finish(false);
            if (e.key === 'Enter' && hideCancel) { e.preventDefault(); finish(true); }
        };
        okBtn.onclick = () => finish(true);
        cancelBtn.onclick = () => finish(false);
        overlay.onclick = (e) => { if (e.target === overlay) finish(false); };
        document.addEventListener('keydown', onKey);
        openModal('confirm-modal');
        (hideCancel ? okBtn : cancelBtn).focus();
    });
}

function isPosProductAvailable(p) {
    if (!p) return false;
    if (p.isService || p.itemType === 'Service' || p.isStockTracked === false) return true;
    return (Number(p.stock) || 0) > 0;
}

async function showOutOfStockPopup(productName, options = {}) {
    const allowSellAnyway = options.allowSellAnyway !== false;
    if (!allowSellAnyway) {
        await confirmDialog(
            tr('pos_out_of_stock_msg').replace('{0}', productName || ''),
            {
                title: tr('pos_out_of_stock_title'),
                confirmText: tr('ok'),
                danger: false,
                hideCancel: true,
            }
        );
        return false;
    }
    return confirmDialog(
        tr('pos_out_of_stock_msg').replace('{0}', productName || ''),
        {
            title: tr('pos_out_of_stock_title'),
            confirmText: tr('pos_sell_anyway'),
            cancelText: tr('cancel'),
            danger: false,
            hideCancel: false,
        }
    );
}

async function showOutOfStockWeightPopup(productName, requestedKg, availableKg, options = {}) {
    const allowSellAnyway = options.allowSellAnyway !== false;
    const req = Number(requestedKg || 0).toFixed(3);
    const avail = Math.max(0, Number(availableKg || 0)).toFixed(3);
    const msg = tr('pos_insufficient_weight_msg')
        .replace('{0}', productName || '')
        .replace('{1}', req)
        .replace('{2}', avail);

    if (!allowSellAnyway) {
        await confirmDialog(msg, {
            title: tr('pos_insufficient_stock_title') || tr('pos_out_of_stock_title'),
            confirmText: tr('ok'),
            danger: false,
            hideCancel: true,
        });
        return false;
    }
    return confirmDialog(msg, {
        title: tr('pos_insufficient_stock_title') || tr('pos_out_of_stock_title'),
        confirmText: tr('pos_sell_anyway'),
        cancelText: tr('cancel'),
        danger: false,
        hideCancel: false,
    });
}

function fillQuickSaleProductList(filter = '') {
    const sel = document.getElementById('qs-product');
    if (!sel) return;
    const q = String(filter || '').trim().toLowerCase();
    if (!q) {
        sel.innerHTML = '';
        sel.hidden = true;
        return;
    }
    sel.hidden = false;
    const list = (products || []).filter(p => {
        if (p.isInactive || p.status === 'Inactive') return false;
        const fields = [p.name, p.sku, p.barcode].map(x => String(x || '').trim().toLowerCase()).filter(Boolean);
        // Match typed letters from the start of the name/sku/barcode, or any word in the name
        return fields.some(f => {
            if (f.startsWith(q)) return true;
            return f.split(/\s+/).some(word => word.startsWith(q));
        });
    }).slice(0, 80);
    sel.innerHTML = list.map(p =>
        `<option value="${p.id}">${escapeHtml(p.name)}</option>`
    ).join('');
    if (list.length) {
        sel.selectedIndex = 0;
        syncQuickSalePriceFromProduct();
    } else {
        const price = document.getElementById('qs-price');
        if (price) price.value = '';
    }
}

function syncQuickSalePriceFromProduct() {
    const id = Number(document.getElementById('qs-product')?.value);
    const p = products.find(x => x.id === id);
    const price = document.getElementById('qs-price');
    if (price && p) price.value = Number(p.price || 0).toFixed(2);
}

function setQuickSaleTab(tab) {
    document.querySelectorAll('.qs-tab').forEach(btn => {
        btn.classList.toggle('active', btn.dataset.qsTab === tab);
    });
    const prod = document.getElementById('qs-panel-product');
    const custom = document.getElementById('qs-panel-custom');
    if (prod) prod.hidden = tab !== 'product';
    if (custom) custom.hidden = tab !== 'custom';
    if (tab === 'product') syncQuickSalePriceFromProduct();
}

function openQuickSaleModal() {
    if (!featureFlags.quickSaleEnabled) {
        toast(tr('feature_quicksale_off'), 'error');
        return;
    }
    setQuickSaleTab('product');
    const search = document.getElementById('qs-product-search');
    if (search) search.value = '';
    const qty = document.getElementById('qs-qty');
    if (qty) qty.value = '1';
    const skip = document.getElementById('qs-skip-stock');
    if (skip) skip.checked = true;
    const customName = document.getElementById('qs-custom-name');
    if (customName) customName.value = '';
    fillQuickSaleProductList(''); // clears list until user types
    const price = document.getElementById('qs-price');
    if (price) price.value = '';
    openModal('quick-sale-modal');
    applyI18n();
    setTimeout(() => (search || document.getElementById('qs-custom-name'))?.focus(), 40);
}

function wireQuickSaleModal() {
    if (wireQuickSaleModal._done) return;
    wireQuickSaleModal._done = true;
    document.querySelectorAll('.qs-tab').forEach(btn => {
        btn.addEventListener('click', () => setQuickSaleTab(btn.dataset.qsTab || 'product'));
    });
    document.getElementById('qs-product-search')?.addEventListener('input', (e) => {
        fillQuickSaleProductList(e.target.value);
    });
    document.getElementById('qs-product')?.addEventListener('change', syncQuickSalePriceFromProduct);
    document.getElementById('btn-qs-add')?.addEventListener('click', () => {
        const tab = document.querySelector('.qs-tab.active')?.dataset.qsTab || 'product';
        const qty = Math.max(1, Math.min(9999, parseInt(document.getElementById('qs-qty')?.value, 10) || 1));
        const priceRaw = document.getElementById('qs-price')?.value;
        const price = Number(priceRaw);
        if (!(price > 0)) {
            toast(tr('qs_need_price'), 'error');
            return;
        }
        if (tab === 'custom') {
            if (!addCustomCartLine(document.getElementById('qs-custom-name')?.value, price, qty)) return;
            closeModal('quick-sale-modal');
            toast(tr('saved_ok'), 'success');
            return;
        }
        const id = Number(document.getElementById('qs-product')?.value);
        const p = products.find(x => x.id === id);
        if (!p) {
            toast(tr('qs_pick_product'), 'error');
            return;
        }
        const skipStock = !!document.getElementById('qs-skip-stock')?.checked || !isPosProductAvailable(p);
        if (isSellByWeight(p)) {
            // Weight items still need weighing; queue the scale with skip-stock intent
            if (!isPosProductAvailable(p) && !skipStock) {
                toast(tr('weigh_first'), 'info');
                return;
            }
            scaleManager.selectForWeighing(p);
            closeModal('quick-sale-modal');
            renderPOS();
            document.getElementById('scaleManualWeight')?.focus();
            toast(tr('weigh_first'), 'info');
            return;
        }
        const ok = addToCart(id, qty, {
            price,
            skipStock,
            allowZeroStock: skipStock
        });
        if (!ok) {
            toast(tr('qs_pick_product'), 'error');
            return;
        }
        closeModal('quick-sale-modal');
        toast(tr('saved_ok'), 'success');
    });
}

function fillCategorySelect(sel) {
    sel.innerHTML = (categories.length ? categories : ['General']).map(c => `<option value="${escapeHtml(c)}">${escapeHtml(c)}</option>`).join('');
}

function fillSupplierSelect(sel) {
    sel.innerHTML = '<option value="">—</option>' + (suppliers || []).map(s =>
        `<option value="${s.id}">${escapeHtml(s.name)}</option>`).join('');
}

function getProductTypeValue() {
    return document.querySelector('input[name="p-type"]:checked')?.value || 'Product';
}

function setProductImagePreview(path) {
    const preview = document.getElementById('p-image-preview');
    const placeholder = document.getElementById('p-image-placeholder');
    const hidden = document.getElementById('p-image');
    const hasImage = !!(path && String(path).trim());
    if (hidden) hidden.value = hasImage ? path : '';
    if (hasImage) {
        const url = path.startsWith('/') || path.startsWith('http') ? path : '/' + path.replace(/^\/+/, '');
        if (preview) { preview.src = url; preview.hidden = false; }
        if (placeholder) placeholder.hidden = true;
    } else {
        if (preview) { preview.src = ''; preview.hidden = true; }
        if (placeholder) placeholder.hidden = false;
        const file = document.getElementById('p-image-file');
        if (file) file.value = '';
    }
    updateProductImageActions(hasImage);
}

function updateProductImageActions(hasImage) {
    const upload = document.getElementById('btn-p-upload');
    const change = document.getElementById('btn-p-change-image');
    const clear = document.getElementById('btn-p-clear-image');
    const remove = document.getElementById('btn-p-remove-image');
    if (upload) upload.hidden = !!hasImage;
    if (change) change.hidden = !hasImage;
    if (clear) clear.hidden = !hasImage;
    if (remove) remove.hidden = !hasImage;
}

function clearProductImage() {
    setProductImagePreview('');
}

function calculateProductMargins() {
    const cost = Number(document.getElementById('p-cost')?.value) || 0;
    [1, 2, 3, 4].forEach(i => {
        const priceEl = document.getElementById(i === 1 ? 'p-price' : 'p-price' + i);
        const grossEl = document.getElementById('p-gross' + i);
        const profitEl = document.getElementById('p-profit' + i);
        const price = Number(priceEl?.value) || 0;
        const profit = price - cost;
        const gross = price > 0 ? (profit / price) * 100 : 0;
        if (profitEl) profitEl.value = profit.toFixed(2);
        if (grossEl) grossEl.value = gross.toFixed(1) + '%';
    });
}

function getSellByValue() {
    const sel = document.querySelector('input[name="p-sell-by"]:checked');
    return sel ? sel.value : 'piece';
}

const DEFAULT_PIECE_UOMS = ['pcs', 'box', 'pack', 'meter', 'liter', 'g'];
let cachedUoms = [...DEFAULT_PIECE_UOMS];

async function loadUoms() {
    try {
        const list = await api('/api/uoms');
        if (Array.isArray(list) && list.length) {
            cachedUoms = list.map(x => String(x || '').trim()).filter(Boolean);
        }
    } catch {
        // Keep cached/default list if API unavailable
    }
    return cachedUoms;
}

function getPieceUomOptions() {
    const set = new Set(DEFAULT_PIECE_UOMS.map(u => u.toLowerCase()));
    const extras = [];
    for (const u of cachedUoms) {
        const key = String(u || '').trim();
        if (!key || key.toLowerCase() === 'kg') continue;
        if (set.has(key.toLowerCase())) continue;
        set.add(key.toLowerCase());
        extras.push(key);
    }
    // Also include units already used on loaded products
    for (const p of (products || [])) {
        const key = String(p?.uom || '').trim();
        if (!key || key.toLowerCase() === 'kg') continue;
        if (set.has(key.toLowerCase())) continue;
        set.add(key.toLowerCase());
        extras.push(key);
    }
    extras.sort((a, b) => a.localeCompare(b, undefined, { sensitivity: 'base' }));
    return [...DEFAULT_PIECE_UOMS, ...extras].map(v => ({ value: v, label: v }));
}

function setProductUomOptions(byWeight, preferred) {
    const uom = document.getElementById('p-uom');
    const addBtn = document.getElementById('btn-p-add-uom');
    if (!uom) return;
    const prev = preferred != null ? preferred : uom.value;
    const isService = getProductTypeValue() === 'Service';
    if (byWeight) {
        uom.innerHTML = '<option value="g">g (grams)</option>';
        uom.value = 'g';
        uom.disabled = true;
        if (addBtn) addBtn.hidden = true;
    } else {
        let opts = getPieceUomOptions();
        const prevKey = String(prev || '').trim();
        if (prevKey && !opts.some(o => o.value.toLowerCase() === prevKey.toLowerCase())) {
            opts = [...opts, { value: prevKey, label: prevKey }];
        }
        uom.innerHTML = opts.map(o => `<option value="${escapeHtml(o.value)}">${escapeHtml(o.label)}</option>`).join('');
        uom.disabled = false;
        if (addBtn) addBtn.hidden = isService;
        const match = opts.find(o => o.value.toLowerCase() === prevKey.toLowerCase());
        uom.value = match ? match.value : 'pcs';
    }
}

function promptDialog(options = {}) {
    const {
        title = tr('add_uom'),
        message = tr('add_uom_hint'),
        confirmText = tr('add'),
        cancelText = tr('cancel'),
        placeholder = '',
        initialValue = '',
        inputType = 'text',
        maxLength = 128,
        autocomplete = 'off',
    } = options;
    const overlay = document.getElementById('prompt-modal');
    const titleEl = document.getElementById('prompt-title');
    const messageEl = document.getElementById('prompt-message');
    const inputEl = document.getElementById('prompt-input');
    const okBtn = document.getElementById('prompt-ok');
    const cancelBtn = document.getElementById('prompt-cancel');
    if (!overlay || !titleEl || !messageEl || !inputEl || !okBtn || !cancelBtn) {
        return Promise.resolve(window.prompt(message, initialValue));
    }

    titleEl.textContent = title;
    messageEl.textContent = message;
    okBtn.textContent = confirmText;
    cancelBtn.textContent = cancelText;
    inputEl.type = inputType === 'password' ? 'password' : 'text';
    inputEl.maxLength = Number(maxLength) > 0 ? Number(maxLength) : 128;
    inputEl.autocomplete = autocomplete || 'off';
    inputEl.placeholder = placeholder;
    inputEl.value = initialValue || '';

    return new Promise(resolve => {
        const finish = (result) => {
            okBtn.onclick = null;
            cancelBtn.onclick = null;
            overlay.onclick = null;
            inputEl.onkeydown = null;
            document.removeEventListener('keydown', onKey);
            closeModal('prompt-modal');
            resolve(result);
        };
        const onKey = (e) => {
            if (e.key === 'Escape') finish(null);
        };
        okBtn.onclick = () => finish(inputEl.value.trim());
        cancelBtn.onclick = () => finish(null);
        overlay.onclick = (e) => { if (e.target === overlay) finish(null); };
        inputEl.onkeydown = (e) => {
            if (e.key === 'Enter') { e.preventDefault(); finish(inputEl.value.trim()); }
        };
        document.addEventListener('keydown', onKey);
        openModal('prompt-modal');
        setTimeout(() => { inputEl.focus(); inputEl.select(); }, 30);
    });
}

async function addCustomUom() {
    if (getSellByValue() === 'weight' || getProductTypeValue() === 'Service') return;
    const name = await promptDialog({
        title: tr('add_uom'),
        message: tr('add_uom_hint'),
        confirmText: tr('add'),
        placeholder: 'carton',
    });
    if (!name) return;
    const cleaned = name.trim().replace(/\s+/g, ' ');
    if (!cleaned) return;

    const existing = getPieceUomOptions().map(o => o.value.toLowerCase());
    if (existing.includes(cleaned.toLowerCase()) || cleaned.toLowerCase() === 'kg') {
        toast(tr('uom_exists'), 'error');
        setProductUomOptions(false, cleaned);
        return;
    }

    try {
        await api('/api/uoms', { method: 'POST', body: JSON.stringify({ name: cleaned }) });
        if (!cachedUoms.some(u => String(u).toLowerCase() === cleaned.toLowerCase())) {
            cachedUoms = [...cachedUoms, cleaned];
        }
        setProductUomOptions(false, cleaned);
        updatePieceStockLabels();
        toast(tr('uom_added'), 'success');
    } catch (e) {
        // Still add locally so the user can save the product
        if (!cachedUoms.some(u => String(u).toLowerCase() === cleaned.toLowerCase())) {
            cachedUoms = [...cachedUoms, cleaned];
        }
        setProductUomOptions(false, cleaned);
        updatePieceStockLabels();
        toast(e.message || tr('uom_added'), e.message ? 'error' : 'success');
    }
}

function updatePieceStockLabels() {
    const uomVal = document.getElementById('p-uom')?.value || 'pcs';
    const stockLabel = document.getElementById('p-stock-label');
    const minLabel = document.getElementById('p-min-stock-label');
    const stockHint = document.getElementById('p-stock-hint');
    if (stockLabel) stockLabel.textContent = `${tr('col_stock')} (${uomVal})`;
    if (minLabel) minLabel.textContent = `${tr('low_level')} (${uomVal})`;
    if (stockHint) {
        stockHint.hidden = false;
        stockHint.textContent = tr('stock_units_hint');
    }
}

function updateSellByWeightUI(preferredUom) {
    const isService = getProductTypeValue() === 'Service';
    const byWeight = !isService && getSellByValue() === 'weight';
    const sellFs = document.getElementById('p-sell-by-fieldset');
    if (sellFs) sellFs.hidden = isService;
    const sellHint = document.getElementById('p-sell-by-hint');
    if (sellHint) {
        sellHint.hidden = byWeight || isService;
        sellHint.textContent = tr('sell_by_hint_piece');
    }
    const guide = document.getElementById('p-weight-guide');
    if (guide) guide.hidden = !byWeight;
    const priceHint = document.getElementById('p-price-hint');
    if (priceHint) priceHint.hidden = !byWeight;
    const priceCol = document.getElementById('p-price-col-label');
    if (priceCol) priceCol.textContent = byWeight ? tr('price_per_kg') : tr('col_price');
    const pricesLegend = document.getElementById('p-prices-legend');
    if (pricesLegend) pricesLegend.textContent = byWeight ? tr('price_per_kg') : tr('prices');

    const uomHint = document.getElementById('p-uom-hint');
    setProductUomOptions(byWeight, preferredUom);
    if (uomHint) {
        uomHint.hidden = isService;
        uomHint.textContent = byWeight ? tr('uom_weight_hint') : tr('uom_piece_hint');
    }

    const stockLabel = document.getElementById('p-stock-label');
    const minLabel = document.getElementById('p-min-stock-label');
    const stockHint = document.getElementById('p-stock-hint');
    if (byWeight) {
        if (stockLabel) stockLabel.textContent = tr('stock_grams');
        if (minLabel) minLabel.textContent = tr('low_level_grams');
        if (stockHint) {
            stockHint.hidden = false;
            stockHint.textContent = tr('stock_grams_hint');
        }
    } else if (!isService) {
        updatePieceStockLabels();
    } else {
        if (stockLabel) stockLabel.textContent = tr('col_stock');
        if (minLabel) minLabel.textContent = tr('low_level');
        if (stockHint) stockHint.hidden = true;
    }
}

function onProductTypeChange(preferredUom) {
    const isService = getProductTypeValue() === 'Service';
    const track = document.getElementById('p-track-stock');
    const stock = document.getElementById('p-stock');
    const minStock = document.getElementById('p-min-stock');
    if (isService) {
        if (track) { track.checked = false; track.disabled = true; }
        const piece = document.querySelector('input[name="p-sell-by"][value="piece"]');
        if (piece) piece.checked = true;
    } else {
        if (track) track.disabled = false;
    }
    const tracked = track?.checked && !isService;
    if (stock) stock.disabled = !tracked;
    if (minStock) minStock.disabled = !tracked;
    updateSellByWeightUI(preferredUom);
}

function generateAutoSku() {
    let cat = document.getElementById('p-category')?.value?.trim() || 'GEN';
    let name = document.getElementById('p-name')?.value?.trim() || 'PRD';
    const catPrefix = cat.length >= 3 ? cat.substring(0, 3).toUpperCase() : cat.toUpperCase().padEnd(3, 'X');
    const namePrefix = name.length >= 3 ? name.substring(0, 3).toUpperCase() : name.toUpperCase().padEnd(3, 'X');
    const now = new Date();
    const ts = String(now.getFullYear()).slice(-2)
        + String(now.getMonth() + 1).padStart(2, '0')
        + String(now.getDate()).padStart(2, '0')
        + String(now.getHours()).padStart(2, '0')
        + String(now.getMinutes()).padStart(2, '0');
    document.getElementById('p-sku').value = `${catPrefix}-${namePrefix}-${ts}`;
}

async function uploadProductImage(file) {
    const fd = new FormData();
    fd.append('file', file);
    const res = await fetch(API + '/api/products/upload-image', { method: 'POST', body: fd });
    if (!res.ok) {
        let err = res.statusText;
        try { const j = await res.json(); err = j.error || j.title || err; } catch {}
        throw new Error(err);
    }
    return res.json();
}

function buildProductPayload() {
    const isService = getProductTypeValue() === 'Service';
    const tracked = document.getElementById('p-track-stock')?.checked && !isService;
    const suppVal = document.getElementById('p-supplier')?.value;
    const expiry = document.getElementById('p-expiry')?.value || '';
    return {
        name: document.getElementById('p-name').value.trim(),
        description: document.getElementById('p-desc')?.value.trim() || '',
        category: document.getElementById('p-category').value,
        price: Number(document.getElementById('p-price').value),
        cost: Number(document.getElementById('p-cost')?.value) || 0,
        stock: tracked ? Number(document.getElementById('p-stock').value) || 0 : 0,
        minStock: tracked ? Number(document.getElementById('p-min-stock')?.value) || 0 : 0,
        barcode: document.getElementById('p-barcode').value.trim(),
        sku: document.getElementById('p-sku').value.trim(),
        image: document.getElementById('p-image')?.value || '',
        location: document.getElementById('p-location')?.value.trim() || '',
        shelf: document.getElementById('p-shelf')?.value.trim() || '',
        uom: document.getElementById('p-uom')?.value || '',
        batch: document.getElementById('p-batch')?.value.trim() || '',
        expiry,
        itemType: getProductTypeValue(),
        isSalesItem: document.getElementById('p-sales')?.checked ?? true,
        isPurchaseItem: document.getElementById('p-purchase')?.checked ?? false,
        isInactive: document.getElementById('p-inactive')?.checked ?? false,
        taxRate: Number(document.getElementById('p-tax')?.value) || 0,
        isStockTracked: tracked,
        sellByWeight: document.querySelector('input[name="p-sell-by"]:checked')?.value === 'weight'
            || ['kg', 'g', 'gram', 'grams', 'kilo', 'kilos', 'kilogram', 'kilograms', 'lb', 'lbs', 'oz'].includes(String(document.getElementById('p-uom')?.value || '').toLowerCase()),
        price2: Number(document.getElementById('p-price2')?.value) || 0,
        price3: Number(document.getElementById('p-price3')?.value) || 0,
        price4: Number(document.getElementById('p-price4')?.value) || 0,
        supplierId: suppVal ? Number(suppVal) : null,
        supplierPurchaseItemId: (() => {
            const v = Number(document.getElementById('p-supplier-purchase-id')?.value);
            return v > 0 ? v : null;
        })()
    };
}

function fillProductForm(p) {
    const isService = p.itemType === 'Service' || p.isService;
    document.getElementById('p-name').value = p.name || '';
    document.getElementById('p-desc').value = p.description || '';
    document.getElementById('p-category').value = p.category || 'General';
    document.getElementById('p-price').value = p.price ?? '';
    document.getElementById('p-price2').value = p.price2 ?? 0;
    document.getElementById('p-price3').value = p.price3 ?? 0;
    document.getElementById('p-price4').value = p.price4 ?? 0;
    document.getElementById('p-cost').value = p.cost ?? 0;
    document.getElementById('p-stock').value = p.stock ?? 0;
    document.getElementById('p-min-stock').value = p.minStock ?? 0;
    document.getElementById('p-barcode').value = p.barcode || '';
    document.getElementById('p-sku').value = p.sku || '';
    document.getElementById('p-location').value = p.location || '';
    document.getElementById('p-shelf').value = p.shelf || '';
    document.getElementById('p-batch').value = p.batch || '';
    document.getElementById('p-expiry').value = p.expiry ? p.expiry.substring(0, 10) : '';
    document.getElementById('p-sales').checked = p.isSalesItem !== false;
    document.getElementById('p-purchase').checked = !!p.isPurchaseItem;
    document.getElementById('p-inactive').checked = !!p.isInactive;
    document.getElementById('p-tax').value = String(p.taxRate ?? 0);
    document.getElementById('p-track-stock').checked = p.isStockTracked !== false && !isService;
    const sellBy = isSellByWeight(p) ? 'weight' : 'piece';
    const sellRadio = document.querySelector(`input[name="p-sell-by"][value="${sellBy}"]`);
    if (sellRadio) sellRadio.checked = true;
    const typeRadio = document.querySelector(`input[name="p-type"][value="${isService ? 'Service' : 'Product'}"]`);
    if (typeRadio) typeRadio.checked = true;
    fillSupplierSelect(document.getElementById('p-supplier'));
    document.getElementById('p-supplier').value = p.supplierId ? String(p.supplierId) : '';
    setProductImagePreview(p.image || '');
    onProductTypeChange(p.uom || '');
    calculateProductMargins();
}

function resetProductForm() {
    document.getElementById('product-form').reset();
    document.getElementById('p-id').value = '';
    const link = document.getElementById('p-supplier-purchase-id');
    if (link) link.value = '';
    document.getElementById('p-sales').checked = true;
    document.getElementById('p-purchase').checked = false;
    document.getElementById('p-inactive').checked = false;
    document.getElementById('p-track-stock').checked = true;
    document.querySelector('input[name="p-type"][value="Product"]').checked = true;
    const sellPiece = document.querySelector('input[name="p-sell-by"][value="piece"]');
    if (sellPiece) sellPiece.checked = true;
    fillCategorySelect(document.getElementById('p-category'));
    fillSupplierSelect(document.getElementById('p-supplier'));
    setProductImagePreview('');
    onProductTypeChange();
    calculateProductMargins();
}

function openProductModal(id) {
    editingProductId = id || null;
    const title = document.getElementById('product-modal-title');
    fillCategorySelect(document.getElementById('p-category'));
    fillSupplierSelect(document.getElementById('p-supplier'));
    const link = document.getElementById('p-supplier-purchase-id');
    if (link) link.value = '';
    if (id) {
        const p = products.find(x => x.id === id);
        if (!p) return;
        const isService = p.itemType === 'Service' || p.isService;
        if (title) title.textContent = tr(isService ? 'edit_service' : 'edit_product');
        document.getElementById('p-id').value = id;
        fillProductForm(p);
        const panel = document.getElementById('p-supplier-purchases');
        if (panel) panel.hidden = true;
    } else {
        if (title) title.textContent = tr('add_product');
        resetProductForm();
        refreshProductSupplierPurchases();
    }
    openModal('product-modal');
}

async function deleteProduct(id) {
    if (!await confirmDialog(tr('confirm_delete'))) return;
    try {
        await api('/api/products/' + id + '/delete', { method: 'POST' });
        toast(tr('deleted_ok'), 'success'); await loadData();
    } catch (e) { toast(e.message, 'error'); }
}

function openStockModal(id) {
    const p = products.find(x => x.id === id);
    if (!p) return;
    document.getElementById('stock-product-id').value = id;
    document.getElementById('stock-change').value = '';
    document.getElementById('stock-reason').value = '';
    openModal('stock-modal');
}

async function submitStockAdjust(e) {
    e.preventDefault();
    const id = document.getElementById('stock-product-id').value;
    const change = Number(document.getElementById('stock-change').value);
    const reason = document.getElementById('stock-reason').value.trim();
    if (!id || !change) return;
    try {
        await api('/api/products/' + id + '/adjust-stock', {
            method: 'POST',
            body: JSON.stringify({ change, reason })
        });
        closeModal('stock-modal');
        toast(tr('stock_ok'), 'success');
        await loadData();
    } catch (err) { toast(err.message, 'error'); }
}

async function openCustomerDetails(id) {
    const c = customers.find(x => x.id === id);
    if (!c) return;
    document.getElementById('cust-pay-id').value = id;
    document.getElementById('cust-pay-amount').value = '';
    document.getElementById('cust-pay-note').value = '';
    document.getElementById('customer-details-info').innerHTML = `
        <div style="line-height:1.8;">
            <div><strong>${tr('col_name')}:</strong> ${escapeHtml(c.name)}</div>
            <div><strong>${tr('col_phone')}:</strong> ${escapeHtml(c.phone || '—')}</div>
            <div><strong>${tr('col_email')}:</strong> ${escapeHtml(c.email || '—')}</div>
            <div><strong>${tr('col_address')}:</strong> ${escapeHtml(c.address || '—')}</div>
            <div><strong>${tr('col_balance')}:</strong> ${money(c.balance)}</div>
            <div><strong>${tr('col_type')}:</strong> ${escapeHtml(c.type || '—')}</div>
        </div>`;
    openModal('customer-details-modal');
    applyI18n();
    await loadCustomerDebtPanel(id);
}

async function loadCustomerDebtPanel(customerId) {
    const panel = document.getElementById('customer-debt-panel');
    const list = document.getElementById('customer-debt-orders');
    const summary = document.getElementById('customer-debt-summary');
    const pays = document.getElementById('customer-debt-payments');
    if (!panel || !list) return;
    panel.hidden = false;
    list.innerHTML = `<div class="empty-state">${tr('loading')}</div>`;
    try {
        const debt = await api('/api/customers/' + customerId + '/debt');
        window._customerDebt = debt;
        const orders = debt.orders || [];
        summary.textContent = tr('debt_orders_owed').replace('{0}', money(debt.ordersRemaining ?? 0))
            + ` · ${tr('col_balance')}: ${money(debt.balance ?? 0)}`;
        if (!orders.length) {
            list.innerHTML = `<div class="empty-state">${tr('debt_no_orders')}</div>`;
        } else {
            list.innerHTML = orders.map(o => {
                const items = (o.items || []).map(it => `
                    <div class="debt-item" data-order-item="${it.orderItemId}" data-order="${o.orderId}" data-remain="${it.remaining}">
                        <input type="checkbox" class="debt-item-check" data-order-item="${it.orderItemId}" data-order="${o.orderId}" data-remain="${it.remaining}">
                        <label class="debt-item-name" title="${escapeHtml(it.name)}">${escapeHtml(it.name)} × ${it.qty}</label>
                        <span class="debt-item-amt">${money(it.remaining)}</span>
                        <input type="number" class="debt-item-pay" min="0" step="0.01" max="${Number(it.remaining).toFixed(2)}"
                            placeholder="0" data-order-item="${it.orderItemId}" data-order="${o.orderId}" data-remain="${it.remaining}">
                    </div>`).join('');
                return `<div class="debt-order" data-order="${o.orderId}" data-remain="${o.remaining}">
                    <div class="debt-order-head">
                        <label>
                            <input type="checkbox" class="debt-order-check" data-order="${o.orderId}" data-remain="${o.remaining}">
                            <span>#${o.orderId}</span>
                        </label>
                        <span class="debt-order-meta">${escapeHtml(o.orderDate || '')}</span>
                        <span class="debt-order-rem">${tr('debt_remaining')}: ${money(o.remaining)}</span>
                        <input type="number" class="debt-order-pay" min="0" step="0.01" max="${Number(o.remaining).toFixed(2)}"
                            placeholder="${tr('debt_pay_order')}" data-order="${o.orderId}" data-remain="${o.remaining}" title="${tr('debt_pay_order')}">
                    </div>
                    <div class="debt-items">${items || `<div class="empty-state" style="padding:0.5rem;">—</div>`}</div>
                </div>`;
            }).join('');
            wireDebtSelectionHandlers();
        }
        updateDebtSelectedTotal();
        if (pays) {
            const recent = debt.recentPayments || [];
            if (recent.length) {
                pays.hidden = false;
                pays.innerHTML = `<h4>${tr('debt_recent_payments')}</h4><ul>` +
                    recent.map(p => `<li><strong>${escapeHtml(p.date || '')}</strong> · ${money(p.amount)} · ${escapeHtml(p.notes || '')}</li>`).join('') +
                    `</ul>`;
            } else {
                pays.hidden = true;
                pays.innerHTML = '';
            }
        }
    } catch (e) {
        list.innerHTML = `<div class="empty-state">${escapeHtml(e.message || tr('debt_no_orders'))}</div>`;
    }
}

function wireDebtSelectionHandlers() {
    const root = document.getElementById('customer-debt-orders');
    if (!root || root.dataset.wired === '1') {
        // re-bind after re-render
    }
    root.querySelectorAll('.debt-order-check').forEach(chk => {
        chk.onchange = () => {
            const oid = chk.dataset.order;
            const remain = Number(chk.dataset.remain) || 0;
            const payInp = root.querySelector(`.debt-order-pay[data-order="${oid}"]`);
            // Order-level only — do not auto-select/fill every line (that settled the whole order)
            if (chk.checked) {
                root.querySelectorAll(`.debt-item-check[data-order="${oid}"]`).forEach(ic => { ic.checked = false; });
                root.querySelectorAll(`.debt-item-pay[data-order="${oid}"]`).forEach(ip => { ip.value = ''; });
                if (payInp && !(Number(payInp.value) > 0)) payInp.value = remain.toFixed(2);
            } else if (payInp) {
                payInp.value = '';
            }
            updateDebtSelectedTotal();
        };
    });
    root.querySelectorAll('.debt-item-check').forEach(chk => {
        chk.onchange = () => {
            const remain = Number(chk.dataset.remain) || 0;
            const payInp = root.querySelector(`.debt-item-pay[data-order-item="${chk.dataset.orderItem}"]`);
            const oid = chk.dataset.order;
            if (chk.checked && payInp && !(Number(payInp.value) > 0)) payInp.value = remain.toFixed(2);
            if (!chk.checked && payInp) payInp.value = '';
            // Item selection wins: clear order-level pay so we don't double-allocate
            if (chk.checked && oid) {
                const oChk = root.querySelector(`.debt-order-check[data-order="${oid}"]`);
                const oPay = root.querySelector(`.debt-order-pay[data-order="${oid}"]`);
                if (oChk) oChk.checked = false;
                if (oPay) oPay.value = '';
            }
            updateDebtSelectedTotal();
        };
    });
    root.querySelectorAll('.debt-order-pay, .debt-item-pay').forEach(inp => {
        inp.oninput = () => {
            const v = Number(inp.value);
            if (inp.classList.contains('debt-order-pay')) {
                const oid = inp.dataset.order;
                if (v > 0) {
                    const chk = root.querySelector(`.debt-order-check[data-order="${oid}"]`);
                    if (chk) chk.checked = true;
                    root.querySelectorAll(`.debt-item-check[data-order="${oid}"]`).forEach(ic => { ic.checked = false; });
                    root.querySelectorAll(`.debt-item-pay[data-order="${oid}"]`).forEach(ip => { ip.value = ''; });
                } else {
                    const chk = root.querySelector(`.debt-order-check[data-order="${oid}"]`);
                    if (chk) chk.checked = false;
                }
            } else {
                const oid = inp.dataset.order;
                if (v > 0) {
                    const chk = root.querySelector(`.debt-item-check[data-order-item="${inp.dataset.orderItem}"]`);
                    if (chk) chk.checked = true;
                    const oChk = root.querySelector(`.debt-order-check[data-order="${oid}"]`);
                    const oPay = root.querySelector(`.debt-order-pay[data-order="${oid}"]`);
                    if (oChk) oChk.checked = false;
                    if (oPay) oPay.value = '';
                } else {
                    const chk = root.querySelector(`.debt-item-check[data-order-item="${inp.dataset.orderItem}"]`);
                    if (chk) chk.checked = false;
                }
            }
            updateDebtSelectedTotal();
        };
    });
}

function collectDebtAllocations() {
    const root = document.getElementById('customer-debt-orders');
    const allocations = [];
    if (!root) return allocations;

    // Prefer explicit item amounts when present
    const itemPays = [...root.querySelectorAll('.debt-item-pay')];
    const ordersWithItemPay = new Set();
    itemPays.forEach(inp => {
        const amt = Number(inp.value);
        if (!(amt > 0)) return;
        const max = Number(inp.dataset.remain) || amt;
        const amount = Math.min(amt, max);
        allocations.push({
            orderId: Number(inp.dataset.order),
            orderItemId: Number(inp.dataset.orderItem),
            amount
        });
        ordersWithItemPay.add(String(inp.dataset.order));
    });

    root.querySelectorAll('.debt-order-pay').forEach(inp => {
        const oid = String(inp.dataset.order);
        if (ordersWithItemPay.has(oid)) return; // already allocating by lines
        const amt = Number(inp.value);
        if (!(amt > 0)) return;
        const max = Number(inp.dataset.remain) || amt;
        allocations.push({
            orderId: Number(oid),
            orderItemId: null,
            amount: Math.min(amt, max)
        });
    });

    return allocations;
}

function updateDebtSelectedTotal() {
    const allocs = collectDebtAllocations();
    const total = allocs.reduce((s, a) => s + (Number(a.amount) || 0), 0);
    const sel = document.getElementById('debt-pay-selected');
    if (sel) sel.value = money(total);
    // Do not auto-write #cust-pay-amount — selection total is shown above; amount is optional override
}

async function submitCustomerDebtPayment() {
    const id = document.getElementById('cust-pay-id')?.value;
    if (!id) return;
    const allocations = collectDebtAllocations();
    const typedAmount = Number(document.getElementById('cust-pay-amount')?.value);
    const note = document.getElementById('cust-pay-note')?.value?.trim() || '';
    let amount = 0;
    if (allocations.length) {
        const selected = allocations.reduce((s, a) => s + a.amount, 0);
        // Use selection total unless user typed a smaller amount to pay (partial)
        amount = (typedAmount > 0 && typedAmount + 0.004 < selected) ? typedAmount : selected;
        // Cap allocation lines to the payment amount (FIFO) so server/UI stay consistent
        if (amount + 0.004 < selected) {
            let left = amount;
            for (const a of allocations) {
                if (left < 0.01) { a.amount = 0; continue; }
                const take = Math.min(a.amount, left);
                a.amount = take;
                left -= take;
            }
            // drop zeroed lines
            for (let i = allocations.length - 1; i >= 0; i--) {
                if (!(allocations[i].amount > 0.004)) allocations.splice(i, 1);
            }
        }
    } else {
        amount = typedAmount;
    }
    if (!(amount > 0)) {
        toast(tr('debt_select_or_amount'), 'error');
        return;
    }
    try {
        await api('/api/customers/' + id + '/payment', {
            method: 'POST',
            body: JSON.stringify({ amount, note, allocations })
        });
        toast(tr('payment_ok'), 'success');
        await loadData();
        const c = customers.find(x => x.id === Number(id));
        if (c) {
            document.getElementById('customer-details-info').innerHTML = `
                <div style="line-height:1.8;">
                    <div><strong>${tr('col_name')}:</strong> ${escapeHtml(c.name)}</div>
                    <div><strong>${tr('col_phone')}:</strong> ${escapeHtml(c.phone || '—')}</div>
                    <div><strong>${tr('col_email')}:</strong> ${escapeHtml(c.email || '—')}</div>
                    <div><strong>${tr('col_address')}:</strong> ${escapeHtml(c.address || '—')}</div>
                    <div><strong>${tr('col_balance')}:</strong> ${money(c.balance)}</div>
                    <div><strong>${tr('col_type')}:</strong> ${escapeHtml(c.type || '—')}</div>
                </div>`;
        }
        document.getElementById('cust-pay-amount').value = '';
        document.getElementById('cust-pay-note').value = '';
        await loadCustomerDebtPanel(Number(id));
        renderCustomers();
    } catch (err) { toast(err.message, 'error'); }
}

function openSupplierDetails(id) {
    const s = suppliers.find(x => x.id === id);
    if (!s) return;
    document.getElementById('supp-pay-id').value = id;
    document.getElementById('supp-pay-amount').value = '';
    document.getElementById('supp-pay-note').value = '';
    document.getElementById('supplier-details-info').innerHTML = `
        <div style="line-height:1.8;">
            <div><strong>${tr('col_name')}:</strong> ${escapeHtml(s.name)}</div>
            <div><strong>${tr('col_contact')}:</strong> ${escapeHtml(s.contact || '—')}</div>
            <div><strong>${tr('col_phone')}:</strong> ${escapeHtml(s.phone || '—')}</div>
            <div><strong>${tr('col_email')}:</strong> ${escapeHtml(s.email || '—')}</div>
            <div><strong>${tr('col_address')}:</strong> ${escapeHtml(s.address || '—')}</div>
            <div><strong>${tr('col_balance')}:</strong> ${money(s.balance ?? 0)}</div>
        </div>`;
    openModal('supplier-details-modal');
}

async function openSupplierPurchases(id) {
    const s = suppliers.find(x => x.id === id);
    if (!s) return;
    document.getElementById('sp-supplier-id').value = id;
    const title = document.getElementById('supplier-purchases-title');
    if (title) title.textContent = `${tr('supplier_debt_products')} — ${s.name}`;
    const sub = document.getElementById('supplier-purchases-sub');
    if (sub) sub.textContent = `${tr('col_balance')}: ${money(s.balance ?? 0)}`;
    clearSupplierPurchaseFormFields();
    fillSpCategoryList();
    wireSupplierPurchaseLookup();
    openModal('supplier-purchases-modal');
    applyI18n();
    await loadSupplierPurchasesList(id);
    setTimeout(() => document.getElementById('sp-name')?.focus(), 40);
}

function clearSupplierPurchaseFormFields() {
    document.getElementById('sp-name').value = '';
    document.getElementById('sp-category').value = '';
    document.getElementById('sp-qty').value = '1';
    document.getElementById('sp-price').value = '0';
    document.getElementById('sp-note').value = '';
    document.getElementById('sp-status').value = 'debt';
    const partId = document.getElementById('sp-part-id');
    if (partId) partId.value = '';
    hideSpSuggest();
}

function fillSpCategoryList() {
    const dl = document.getElementById('sp-category-list');
    if (!dl) return;
    const cats = Array.isArray(categories) ? categories : [];
    dl.innerHTML = cats.map(c => `<option value="${escapeHtml(typeof c === 'string' ? c : (c.name || c))}">`).join('');
}

function hideSpSuggest() {
    const box = document.getElementById('sp-suggest');
    if (box) {
        box.hidden = true;
        box.innerHTML = '';
    }
}

function searchProductsForSupplierLookup(query) {
    const q = String(query || '').trim().toLowerCase();
    const list = (products || []).filter(p => {
        if (p.isInactive || p.status === 'Inactive') return false;
        if (!q) return false;
        return [p.name, p.sku, p.barcode, p.category].some(x => String(x || '').toLowerCase().includes(q));
    });
    // Prefer exact barcode/sku matches first
    const exact = findProductByScan(query);
    if (exact && !list.some(x => x.id === exact.id)) list.unshift(exact);
    list.sort((a, b) => {
        const an = String(a.name || '').toLowerCase();
        const bn = String(b.name || '').toLowerCase();
        const aExact = an === q || String(a.barcode || '').toLowerCase() === q || String(a.sku || '').toLowerCase() === q;
        const bExact = bn === q || String(b.barcode || '').toLowerCase() === q || String(b.sku || '').toLowerCase() === q;
        if (aExact !== bExact) return aExact ? -1 : 1;
        return an.localeCompare(bn);
    });
    return list.slice(0, 12);
}

function applyInventoryProductToSupplierForm(p) {
    if (!p) return;
    document.getElementById('sp-name').value = p.name || '';
    document.getElementById('sp-category').value = p.category || 'General';
    const cost = Number(p.cost ?? p.purchasePrice ?? 0);
    const price = cost > 0 ? cost : Number(p.price ?? 0);
    document.getElementById('sp-price').value = price > 0 ? price.toFixed(2) : '0';
    const partId = document.getElementById('sp-part-id');
    if (partId) partId.value = String(p.id || '');
    if (!(Number(document.getElementById('sp-qty').value) > 0))
        document.getElementById('sp-qty').value = '1';
    hideSpSuggest();
}

function renderSpSuggest(query) {
    const box = document.getElementById('sp-suggest');
    if (!box) return;
    const q = String(query || '').trim();
    if (q.length < 1) {
        hideSpSuggest();
        return;
    }
    const list = searchProductsForSupplierLookup(q);
    if (!list.length) {
        box.hidden = false;
        box.innerHTML = `<div class="sp-suggest-item" style="cursor:default;color:var(--text-muted);">${escapeHtml(tr('empty_list'))} — ${escapeHtml(tr('add'))}</div>`;
        return;
    }
    box.hidden = false;
    box.innerHTML = list.map((p, i) => `
        <button type="button" class="sp-suggest-item${i === 0 ? ' active' : ''}" data-sp-pick="${p.id}">
            <span>
                <strong>${escapeHtml(p.name)}</strong>
                <div class="sp-suggest-meta">${escapeHtml(p.category || '')}${p.barcode ? ' · ' + escapeHtml(p.barcode) : ''}${p.sku ? ' · ' + escapeHtml(p.sku) : ''}</div>
            </span>
            <span class="sp-suggest-meta">${money(p.cost > 0 ? p.cost : p.price)}</span>
        </button>`).join('');
    box.querySelectorAll('[data-sp-pick]').forEach(btn => {
        btn.onclick = () => {
            const p = products.find(x => x.id === Number(btn.dataset.spPick));
            applyInventoryProductToSupplierForm(p);
            document.getElementById('sp-qty')?.focus();
        };
    });
}

function trySpBarcodeOrSelect(query) {
    const exact = findProductByScan(query);
    if (exact) {
        applyInventoryProductToSupplierForm(exact);
        document.getElementById('sp-qty')?.focus();
        return true;
    }
    const list = searchProductsForSupplierLookup(query);
    if (list.length === 1) {
        applyInventoryProductToSupplierForm(list[0]);
        document.getElementById('sp-qty')?.focus();
        return true;
    }
    renderSpSuggest(query);
    return false;
}

function wireSupplierPurchaseLookup() {
    if (wireSupplierPurchaseLookup._done) return;
    wireSupplierPurchaseLookup._done = true;
    const input = document.getElementById('sp-name');
    if (!input) return;
    let debounce = null;
    input.addEventListener('input', () => {
        const partId = document.getElementById('sp-part-id');
        if (partId) partId.value = '';
        clearTimeout(debounce);
        debounce = setTimeout(() => renderSpSuggest(input.value), 120);
    });
    input.addEventListener('keydown', (e) => {
        const box = document.getElementById('sp-suggest');
        const items = [...(box?.querySelectorAll('[data-sp-pick]') || [])];
        const active = box?.querySelector('.sp-suggest-item.active');
        let idx = items.indexOf(active);
        if (e.key === 'ArrowDown' && items.length) {
            e.preventDefault();
            items.forEach(x => x.classList.remove('active'));
            idx = Math.min(items.length - 1, idx + 1);
            if (idx < 0) idx = 0;
            items[idx].classList.add('active');
            items[idx].scrollIntoView({ block: 'nearest' });
            return;
        }
        if (e.key === 'ArrowUp' && items.length) {
            e.preventDefault();
            items.forEach(x => x.classList.remove('active'));
            idx = Math.max(0, idx - 1);
            items[idx].classList.add('active');
            items[idx].scrollIntoView({ block: 'nearest' });
            return;
        }
        if (e.key === 'Escape') {
            hideSpSuggest();
            return;
        }
        if (e.key === 'Enter') {
            e.preventDefault();
            if (active && !box?.hidden) {
                active.click();
                return;
            }
            trySpBarcodeOrSelect(input.value);
        }
    });
    input.addEventListener('blur', () => {
        // allow click on suggestion
        setTimeout(() => hideSpSuggest(), 180);
    });
    document.addEventListener('click', (e) => {
        if (!e.target.closest('.sp-lookup')) hideSpSuggest();
    });
}

async function importSupplierPurchaseToInventory(itemId, supplierIdHint) {
    let it = null;
    const sidHint = Number(supplierIdHint) || 0;
    if (sidHint > 0) {
        const items = await api('/api/suppliers/' + sidHint + '/purchases?unadded=1');
        it = (items || []).find(x => x.id === Number(itemId));
        if (!it) {
            const all = await api('/api/suppliers/' + sidHint + '/purchases');
            it = (all || []).find(x => x.id === Number(itemId));
        }
    }
    if (!it) {
        for (const s of (suppliers || [])) {
            try {
                const items = await api('/api/suppliers/' + s.id + '/purchases');
                it = (items || []).find(x => x.id === Number(itemId));
                if (it) break;
            } catch { /* continue */ }
        }
    }
    if (!it) throw new Error(tr('empty_list'));
    startFsImportQueue([it], Number(it.supplierId) || sidHint);
}

/** Queue of supplier purchase lines to open in Add/Edit Product. Line leaves list only after Save (LinkToPart). */
let fsImportQueue = null;

function startFsImportQueue(items, supplierId) {
    const list = (items || []).filter(Boolean);
    if (!list.length) return;
    fsImportQueue = {
        supplierId: Number(supplierId) || Number(list[0].supplierId) || 0,
        items: list,
        index: 0,
        awaitingSave: false
    };
    closeModal('from-supplier-modal');
    closeModal('supplier-purchases-modal');
    openNextFsImportItem();
}

function openNextFsImportItem() {
    if (!fsImportQueue) return;
    if (fsImportQueue.index >= fsImportQueue.items.length) {
        const sid = fsImportQueue.supplierId;
        fsImportQueue = null;
        reopenFromSupplierAfterQueue(sid);
        return;
    }
    const it = fsImportQueue.items[fsImportQueue.index];
    fsImportQueue.awaitingSave = true;
    openSupplierPurchaseInProductForm(it);
}

function reopenFromSupplierAfterQueue(sid) {
    if (!sid) return;
    openFromSupplierModal();
    const sel = document.getElementById('fs-supplier');
    if (sel) sel.value = String(sid);
    loadFromSupplierList();
}

/** Called after product form Save succeeds while importing from supplier. */
function onFsImportProductSaved() {
    if (!fsImportQueue) return;
    fsImportQueue.awaitingSave = false;
    fsImportQueue.index += 1;
    openNextFsImportItem();
}

/** Called when product modal closes without Save — keep line visible, continue or finish queue. */
function onFsImportProductCancelled() {
    if (!fsImportQueue || !fsImportQueue.awaitingSave) return;
    fsImportQueue.awaitingSave = false;
    fsImportQueue.index += 1;
    openNextFsImportItem();
}

function findProductByNameAndSupplier(name, supplierId) {
    const n = String(name || '').trim().toLowerCase();
    const sid = Number(supplierId);
    if (!n || !sid) return null;
    return (products || []).find(p =>
        String(p.name || '').trim().toLowerCase() === n &&
        Number(p.supplierId) === sid &&
        !p.isInactive && p.status !== 'Inactive'
    ) || null;
}

function ensureCategoryOption(catName) {
    const cat = document.getElementById('p-category');
    if (!cat || !catName) return;
    fillCategorySelect(cat);
    if (cat.value === catName) return;
    const exists = [...cat.options].some(o => o.value === catName);
    if (!exists) {
        const opt = document.createElement('option');
        opt.value = catName;
        opt.textContent = catName;
        cat.appendChild(opt);
    }
    cat.value = catName;
}

function openSupplierPurchaseInProductForm(it) {
    const supplierId = Number(it.supplierId);
    const qtyAdd = Math.max(1, Math.round(Number(it.quantity) || 1));
    const unitPrice = Number(it.unitPrice) || 0;
    const catName = (it.category || 'General').trim() || 'General';

    const existing = (it.partId && products.find(p => p.id === Number(it.partId)))
        || findProductByNameAndSupplier(it.name, supplierId);

    if (existing) {
        openProductModal(existing.id);
        document.getElementById('p-supplier-purchase-id').value = String(it.id);
        document.getElementById('p-stock').value = (Number(existing.stock) || 0) + qtyAdd;
        document.getElementById('p-cost').value = unitPrice > 0 ? unitPrice : (existing.cost ?? 0);
        ensureCategoryOption(catName);
        const sel = document.getElementById('p-supplier');
        if (sel && supplierId) sel.value = String(supplierId);
        document.getElementById('p-purchase').checked = true;
        document.getElementById('p-track-stock').checked = true;
        if (typeof calculateProductMargins === 'function') calculateProductMargins();
    } else {
        openProductModal(null);
        document.getElementById('p-supplier-purchase-id').value = String(it.id);
        document.getElementById('p-name').value = it.name || '';
        ensureCategoryOption(catName);
        document.getElementById('p-stock').value = qtyAdd;
        document.getElementById('p-cost').value = unitPrice;
        const sell = document.getElementById('p-price');
        if (sell && !(Number(sell.value) > 0) && unitPrice > 0)
            sell.value = (Math.round(unitPrice * 1.25 * 100) / 100).toFixed(2);
        const sel = document.getElementById('p-supplier');
        if (sel && supplierId) sel.value = String(supplierId);
        document.getElementById('p-purchase').checked = true;
        document.getElementById('p-track-stock').checked = true;
        document.getElementById('p-sales').checked = true;
        if (typeof onProductTypeChange === 'function') onProductTypeChange();
        if (typeof calculateProductMargins === 'function') calculateProductMargins();
    }
}

async function loadSupplierPurchasesList(supplierId) {
    const el = document.getElementById('supplier-purchases-list');
    if (!el) return;
    el.innerHTML = `<div class="empty-state">${tr('loading')}</div>`;
    try {
        const items = await api('/api/suppliers/' + supplierId + '/purchases');
        if (!items?.length) {
            el.innerHTML = `<div class="empty-state">${tr('empty_list')}</div>`;
            return;
        }
        el.innerHTML = items.map(it => {
            const status = String(it.paymentStatus || 'Unpaid').toLowerCase();
            const isPaid = status === 'paid';
            const badge = isPaid ? tr('paid') : (status === 'partial' ? 'Partial' : tr('unpaid'));
            const badgeCls = isPaid ? 'in-stock' : 'low-stock';
            return `<div class="supp-purchase-row" data-id="${it.id}">
                <div>
                    <strong>${escapeHtml(it.name)}</strong>
                    <div class="supp-purchase-meta">
                        ${tr('col_category')}: ${escapeHtml(it.category || 'General')}
                        · ${tr('col_qty')}: ${Number(it.quantity)} · ${tr('col_price')}: ${money(it.unitPrice)} · ${tr('col_amount')}: ${money(it.lineTotal)}
                        · <span class="badge ${badgeCls}">${badge}</span>
                        · ${it.addedToInventory ? tr('supplier_added_inv') : tr('supplier_not_added')}
                        ${it.remaining > 0.004 ? ` · ${tr('debt_remaining')}: ${money(it.remaining)}` : ''}
                    </div>
                </div>
                <div class="supp-purchase-actions">
                    <button type="button" class="btn btn-primary btn-sm" data-sp-import="${it.id}">${tr('supplier_import_inv')}</button>
                    ${it.remaining > 0.004
                        ? `<button type="button" class="btn btn-secondary btn-sm" data-sp-pay="${it.id}">${tr('supplier_mark_paid')}</button>`
                        : `<button type="button" class="btn btn-secondary btn-sm" data-sp-debt="${it.id}">${tr('supplier_mark_debt')}</button>`}
                    <button type="button" class="btn-icon btn-icon-danger" title="${tr('delete')}" data-sp-del="${it.id}"><span class="material-symbols-rounded">delete</span></button>
                </div>
            </div>`;
        }).join('');

        el.querySelectorAll('[data-sp-import]').forEach(btn => btn.onclick = async () => {
            try {
                await importSupplierPurchaseToInventory(Number(btn.dataset.spImport), supplierId);
            } catch (e) { toast(e.message, 'error'); }
        });
        el.querySelectorAll('[data-sp-pay]').forEach(btn => btn.onclick = async () => {
            try {
                await api('/api/supplier-purchases/' + btn.dataset.spPay + '/pay', { method: 'POST', body: JSON.stringify({}) });
                toast(tr('payment_ok'), 'success');
                await loadData();
                await loadSupplierPurchasesList(supplierId);
                const s = suppliers.find(x => x.id === supplierId);
                const sub = document.getElementById('supplier-purchases-sub');
                if (sub && s) sub.textContent = `${tr('col_balance')}: ${money(s.balance ?? 0)}`;
            } catch (e) { toast(e.message, 'error'); }
        });
        el.querySelectorAll('[data-sp-debt]').forEach(btn => btn.onclick = async () => {
            try {
                await api('/api/supplier-purchases/' + btn.dataset.spDebt + '/debt', { method: 'POST' });
                toast(tr('saved_ok'), 'success');
                await loadData();
                await loadSupplierPurchasesList(supplierId);
                const s = suppliers.find(x => x.id === supplierId);
                const sub = document.getElementById('supplier-purchases-sub');
                if (sub && s) sub.textContent = `${tr('col_balance')}: ${money(s.balance ?? 0)}`;
            } catch (e) { toast(e.message, 'error'); }
        });
        el.querySelectorAll('[data-sp-del]').forEach(btn => btn.onclick = async () => {
            if (!await confirmDialog(tr('confirm_delete'))) return;
            try {
                await api('/api/supplier-purchases/' + btn.dataset.spDel + '/delete', { method: 'POST' });
                toast(tr('deleted_ok'), 'success');
                await loadData();
                await loadSupplierPurchasesList(supplierId);
            } catch (e) { toast(e.message, 'error'); }
        });
    } catch (e) {
        el.innerHTML = `<div class="empty-state">${escapeHtml(e.message || tr('empty_list'))}</div>`;
    }
}

async function addSupplierPurchaseLine() {
    const supplierId = Number(document.getElementById('sp-supplier-id')?.value);
    const name = document.getElementById('sp-name')?.value?.trim();
    const category = document.getElementById('sp-category')?.value?.trim() || 'General';
    const qty = Number(document.getElementById('sp-qty')?.value) || 1;
    const price = Number(document.getElementById('sp-price')?.value) || 0;
    const note = document.getElementById('sp-note')?.value?.trim() || '';
    const isPaid = document.getElementById('sp-status')?.value === 'paid';
    if (!supplierId || !name) {
        toast(tr('col_name'), 'error');
        return;
    }
    try {
        await api('/api/suppliers/' + supplierId + '/purchases', {
            method: 'POST',
            body: JSON.stringify({ name, category, quantity: qty, unitPrice: price, isPaid, notes: note })
        });
        toast(tr('supplier_purchase_ok'), 'success');
        clearSupplierPurchaseFormFields();
        await loadData();
        await loadSupplierPurchasesList(supplierId);
        const s = suppliers.find(x => x.id === supplierId);
        const sub = document.getElementById('supplier-purchases-sub');
        if (sub && s) sub.textContent = `${tr('col_balance')}: ${money(s.balance ?? 0)}`;
        setTimeout(() => document.getElementById('sp-name')?.focus(), 40);
    } catch (e) { toast(e.message, 'error'); }
}

function applySupplierPurchaseToProductForm(it, supplierId) {
    document.getElementById('p-name').value = it.name || '';
    document.getElementById('p-cost').value = Number(it.unitPrice) || 0;
    document.getElementById('p-stock').value = Number(it.quantity) || 0;
    const cat = document.getElementById('p-category');
    if (cat && it.category) {
        fillCategorySelect(cat);
        cat.value = it.category;
        if (cat.value !== it.category) {
            // category may not exist yet — leave and let save create via payload
            const opt = document.createElement('option');
            opt.value = it.category;
            opt.textContent = it.category;
            cat.appendChild(opt);
            cat.value = it.category;
        }
    }
    const track = document.getElementById('p-track-stock');
    if (track) track.checked = true;
    const sel = document.getElementById('p-supplier');
    if (sel && supplierId) sel.value = String(supplierId);
    const link = document.getElementById('p-supplier-purchase-id');
    if (link) link.value = String(it.id || '');
    const purchase = document.getElementById('p-purchase');
    if (purchase) purchase.checked = true;
    if (typeof onProductTypeChange === 'function') onProductTypeChange();
    if (typeof calculateProductMargins === 'function') calculateProductMargins();
    refreshProductSupplierPurchases();
    document.querySelectorAll('.p-supplier-purchase-chip').forEach(c => {
        c.classList.toggle('active', Number(c.dataset.id) === Number(it.id));
    });
}

async function refreshProductSupplierPurchases() {
    const panel = document.getElementById('p-supplier-purchases');
    const list = document.getElementById('p-supplier-purchases-list');
    const sid = document.getElementById('p-supplier')?.value;
    const purchaseIdEl = document.getElementById('p-supplier-purchase-id');
    if (!panel || !list) return;
    if (!sid || editingProductId) {
        panel.hidden = true;
        list.innerHTML = '';
        if (purchaseIdEl && !editingProductId) purchaseIdEl.value = '';
        return;
    }
    try {
        const items = await api('/api/suppliers/' + sid + '/purchases');
        if (!items?.length) {
            panel.hidden = true;
            list.innerHTML = '';
            return;
        }
        panel.hidden = false;
        const selectedId = Number(purchaseIdEl?.value) || 0;
        list.innerHTML = items.map(it => `
            <button type="button" class="p-supplier-purchase-chip ${selectedId === it.id ? 'active' : ''}" data-id="${it.id}">
                <span>${escapeHtml(it.name)} · ${escapeHtml(it.category || '')} · ${tr('col_qty')}: ${Number(it.quantity)}</span>
                <span>${money(it.unitPrice)}</span>
            </button>`).join('');
        list.querySelectorAll('.p-supplier-purchase-chip').forEach(btn => {
            btn.onclick = () => {
                const it = items.find(x => x.id === Number(btn.dataset.id));
                if (it) applySupplierPurchaseToProductForm(it, Number(sid));
            };
        });
    } catch {
        panel.hidden = true;
        list.innerHTML = '';
    }
}

function updateInvActionsSlide() {
    const scroller = document.getElementById('inv-actions-scroller');
    const slide = document.getElementById('inv-actions-slide');
    const thumb = document.getElementById('inv-actions-slide-thumb');
    if (!scroller || !slide || !thumb) return;
    const maxScroll = Math.max(0, scroller.scrollWidth - scroller.clientWidth);
    const needs = maxScroll > 2;
    slide.classList.toggle('is-scrollable', needs);
    slide.style.display = needs ? 'block' : 'none';
    if (!needs) return;

    const trackW = slide.clientWidth || scroller.clientWidth;
    if (!trackW) return;

    const isRtl = document.body.classList.contains('rtl') || getComputedStyle(scroller).direction === 'rtl';
    const ratio = scroller.clientWidth / Math.max(1, scroller.scrollWidth);
    const thumbW = Math.max(28, trackW * ratio);
    const maxLeft = Math.max(0, trackW - thumbW);

    let sl = scroller.scrollLeft;
    if (isRtl) sl = Math.abs(sl);
    sl = Math.min(maxScroll, Math.max(0, sl));

    const left = maxScroll > 0 ? (sl / maxScroll) * maxLeft : 0;
    thumb.style.width = thumbW + 'px';
    thumb.style.transform = `translateX(${isRtl ? -left : left}px)`;
}

function initInvActionsSlide() {
    const scroller = document.getElementById('inv-actions-scroller');
    const slide = document.getElementById('inv-actions-slide');
    if (!scroller || !slide || slide.dataset.bound === '1') return;
    slide.dataset.bound = '1';

    const isRtl = () => document.body.classList.contains('rtl') || getComputedStyle(scroller).direction === 'rtl';

    const scrollToRatio = (ratio) => {
        const maxScroll = scroller.scrollWidth - scroller.clientWidth;
        if (maxScroll <= 2) return;
        const clamped = Math.min(1, Math.max(0, ratio));
        const target = clamped * maxScroll;
        scroller.scrollLeft = isRtl() ? -target : target;
        updateInvActionsSlide();
    };

    scroller.addEventListener('scroll', updateInvActionsSlide, { passive: true });
    scroller.addEventListener('wheel', e => {
        if (scroller.scrollWidth <= scroller.clientWidth + 2) return;
        e.preventDefault();
        const delta = Math.abs(e.deltaX) > Math.abs(e.deltaY) ? e.deltaX : e.deltaY;
        scroller.scrollLeft += (isRtl() ? -delta : delta);
        updateInvActionsSlide();
    }, { passive: false });

    let isDragScroller = false, startX = 0, startSl = 0;
    scroller.addEventListener('pointerdown', e => {
        if (e.target.closest('button, input, select, a')) return;
        isDragScroller = true;
        startX = e.clientX;
        startSl = scroller.scrollLeft;
        scroller.setPointerCapture?.(e.pointerId);
    });
    scroller.addEventListener('pointermove', e => {
        if (!isDragScroller) return;
        const dx = e.clientX - startX;
        scroller.scrollLeft = startSl - dx;
        updateInvActionsSlide();
    });
    const stopScrollerDrag = () => { isDragScroller = false; };
    scroller.addEventListener('pointerup', stopScrollerDrag);
    scroller.addEventListener('pointercancel', stopScrollerDrag);

    let dragging = false;
    slide.addEventListener('pointerdown', e => {
        if (scroller.scrollWidth <= scroller.clientWidth + 2) return;
        dragging = true;
        slide.setPointerCapture?.(e.pointerId);
        const rect = slide.getBoundingClientRect();
        let r = (e.clientX - rect.left) / rect.width;
        if (isRtl()) r = 1 - r;
        scrollToRatio(r);
        e.preventDefault();
    });
    slide.addEventListener('pointermove', e => {
        if (!dragging) return;
        const rect = slide.getBoundingClientRect();
        let r = (e.clientX - rect.left) / rect.width;
        if (isRtl()) r = 1 - r;
        scrollToRatio(r);
    });
    const endDrag = () => { dragging = false; };
    slide.addEventListener('pointerup', endDrag);
    slide.addEventListener('pointercancel', endDrag);

    window.addEventListener('resize', updateInvActionsSlide);
    if (typeof ResizeObserver !== 'undefined') {
        new ResizeObserver(updateInvActionsSlide).observe(scroller);
        new ResizeObserver(updateInvActionsSlide).observe(slide);
    }

    requestAnimationFrame(updateInvActionsSlide);
    setTimeout(updateInvActionsSlide, 50);
    setTimeout(updateInvActionsSlide, 300);
}

// Prevent browser/viewport zooming (Ctrl+Wheel, Ctrl++, Ctrl+-, Pinch)
window.addEventListener('wheel', e => {
    if (e.ctrlKey || e.metaKey) {
        e.preventDefault();
    }
}, { passive: false });

window.addEventListener('keydown', e => {
    if ((e.ctrlKey || e.metaKey) && (e.key === '+' || e.key === '-' || e.key === '=' || e.key === '_' || e.key === '0' || e.code === 'NumpadAdd' || e.code === 'NumpadSubtract')) {
        e.preventDefault();
    }
});

window.addEventListener('gesturestart', e => e.preventDefault());
window.addEventListener('gesturechange', e => e.preventDefault());

function openFromSupplierModal() {
    fillSupplierSelect(document.getElementById('fs-supplier'));
    document.getElementById('fs-list').innerHTML = `<div class="empty-state">${tr('supplier_pick_supplier')}</div>`;
    const tb = document.getElementById('fs-toolbar');
    if (tb) tb.hidden = true;
    const all = document.getElementById('fs-select-all');
    if (all) all.checked = false;
    openModal('from-supplier-modal');
    applyI18n();
}

async function loadFromSupplierList() {
    const sid = document.getElementById('fs-supplier')?.value;
    const el = document.getElementById('fs-list');
    const tb = document.getElementById('fs-toolbar');
    if (!el) return;
    if (!sid) {
        el.innerHTML = `<div class="empty-state">${tr('supplier_pick_supplier')}</div>`;
        if (tb) tb.hidden = true;
        return;
    }
    el.innerHTML = `<div class="empty-state">${tr('loading')}</div>`;
    if (tb) tb.hidden = true;
    try {
        const items = await api('/api/suppliers/' + sid + '/purchases?unadded=1');
        window._fsListCache = items || [];
        if (!items?.length) {
            el.innerHTML = `<div class="empty-state">${tr('empty_list')}</div>`;
            return;
        }
        if (tb) tb.hidden = false;
        el.innerHTML = items.map(it => `
            <div class="supp-purchase-row" data-fs-id="${it.id}">
                <input type="checkbox" class="fs-check" data-fs-check="${it.id}" aria-label="select">
                <div>
                    <strong>${escapeHtml(it.name)}</strong>
                    <div class="supp-purchase-meta">
                        ${tr('col_category')}: ${escapeHtml(it.category || 'General')}
                        · ${tr('col_qty')}: ${Number(it.quantity)} · ${money(it.unitPrice)}
                    </div>
                </div>
                <div class="supp-purchase-actions">
                    <button type="button" class="btn btn-primary btn-sm" data-fs-import="${it.id}">${tr('supplier_import_inv')}</button>
                </div>
            </div>`).join('');
        const syncSelectAll = () => {
            const boxes = [...el.querySelectorAll('[data-fs-check]')];
            const allEl = document.getElementById('fs-select-all');
            if (allEl) allEl.checked = boxes.length > 0 && boxes.every(b => b.checked);
        };
        el.querySelectorAll('[data-fs-check]').forEach(cb => cb.onchange = syncSelectAll);
        el.querySelectorAll('[data-fs-import]').forEach(btn => btn.onclick = async () => {
            try {
                await importSupplierPurchaseToInventory(Number(btn.dataset.fsImport), sid);
            } catch (e) { toast(e.message, 'error'); }
        });
        syncSelectAll();
    } catch (e) {
        el.innerHTML = `<div class="empty-state">${escapeHtml(e.message || tr('empty_list'))}</div>`;
    }
}

function importSelectedFromSupplier() {
    const sid = Number(document.getElementById('fs-supplier')?.value) || 0;
    const el = document.getElementById('fs-list');
    if (!sid || !el) return;
    const ids = [...el.querySelectorAll('[data-fs-check]:checked')].map(cb => Number(cb.dataset.fsCheck));
    if (!ids.length) {
        toast(tr('supplier_none_selected'), 'error');
        return;
    }
    const cache = window._fsListCache || [];
    const selected = ids.map(id => cache.find(x => x.id === id)).filter(Boolean);
    if (!selected.length) {
        toast(tr('supplier_none_selected'), 'error');
        return;
    }
    startFsImportQueue(selected, sid);
}

function renderCategoriesManage() {
    const el = document.getElementById('categories-manage-list');
    if (!el) return;
    el.innerHTML = categories.length ? categories.map(c => `
        <div class="manage-list-item">
            <span>${escapeHtml(c)}</span>
            <div class="table-actions">
                <button type="button" class="btn btn-secondary btn-sm" data-rename-cat="${escapeHtml(c)}">${tr('rename')}</button>
                <button type="button" class="btn-icon btn-icon-danger" title="${tr('delete')}" data-del-cat="${escapeHtml(c)}"><span class="material-symbols-rounded">delete</span></button>
            </div>
        </div>`).join('') : `<div class="empty-state">${tr('empty_list')}</div>`;
    el.querySelectorAll('[data-rename-cat]').forEach(btn => btn.onclick = () => {
        const oldName = btn.getAttribute('data-rename-cat');
        document.getElementById('cat-old-name').value = oldName;
        document.getElementById('cat-name').value = oldName;
        closeModal('categories-manage-modal');
        openModal('category-modal');
    });
    el.querySelectorAll('[data-del-cat]').forEach(btn => btn.onclick = () => deleteCategory(btn.getAttribute('data-del-cat')));
}

async function deleteCategory(name) {
    if (!await confirmDialog(tr('confirm_delete'))) return;
    try {
        await api('/api/categories/delete', { method: 'POST', body: JSON.stringify({ name }) });
        toast(tr('deleted_ok'), 'success');
        await loadData();
        renderCategoriesManage();
    } catch (e) { toast(e.message, 'error'); }
}

function renderExpenseCategoriesList() {
    const el = document.getElementById('exp-cats-list');
    if (!el) return;
    const cats = expenseCategories.length ? expenseCategories : ['Other'];
    el.innerHTML = cats.map(c => `
        <div class="manage-list-item">
            <span>${escapeHtml(c)}</span>
            <button type="button" class="btn-icon btn-icon-danger" title="${tr('delete')}" data-del-exp-cat="${escapeHtml(c)}"><span class="material-symbols-rounded">delete</span></button>
        </div>`).join('');
    el.querySelectorAll('[data-del-exp-cat]').forEach(btn => btn.onclick = () => deleteExpenseCategory(btn.getAttribute('data-del-exp-cat')));
}

async function deleteExpenseCategory(name) {
    if (!await confirmDialog(tr('confirm_delete'))) return;
    try {
        await api('/api/expense-categories/delete', { method: 'POST', body: JSON.stringify({ name }) });
        toast(tr('deleted_ok'), 'success');
        expenseCategories = await api('/api/expense-categories').catch(() => []);
        renderExpenseCategoriesList();
    } catch (e) { toast(e.message, 'error'); }
}

async function fillExpenseCategorySelect() {
    let cats = expenseCategories;
    if (!cats?.length) {
        try { cats = await api('/api/expense-categories'); expenseCategories = cats; } catch { cats = ['Other']; }
    }
    const select = document.getElementById('e-category');
    if (!select) return;
    select.innerHTML = cats.map(c => `<option>${escapeHtml(c)}</option>`).join('') +
        `<option value="__add_new__">${escapeHtml(tr('add_expense_category_option'))}</option>`;
}

async function placeOrder(status, opts = {}) {
    if (!cart.length) return toast(tr('empty_cart_hint'), 'error');
    const customerSel = document.getElementById('pos-customer');
    const customerId = customerSel?.value ? Number(customerSel.value) : null;
    if (opts.requireCustomer && !customerId) return toast(tr('bill_need_customer'), 'error');
    const t = getPosTotals();
    const cust = customerId ? customers.find(x => x.id === customerId) : null;
    if (opts.isPaid === false && cust) {
        const limit = Number(cust.creditLimit);
        if (Number.isFinite(limit) && limit > 0) {
            const newBal = (Number(cust.balance) || 0) + t.total;
            if (newBal > limit + 0.004) {
                const msg = tr('credit_limit_warn')
                    .replace('{0}', money(limit))
                    .replace('{1}', money(newBal));
                if (!await confirmDialog(msg, { danger: false, confirmText: tr('confirm_btn') })) return null;
            }
        }
    }
    let vatAmount = t.vat;
    let shippingAmount = t.ship;
    let discountAmount = t.disc;
    const delta = +(t.total - t.calc).toFixed(2);
    if (delta < -0.004) discountAmount = +(discountAmount - delta).toFixed(2);
    else if (delta > 0.004) shippingAmount = +(shippingAmount + delta).toFixed(2);
    const payload = {
        items: cart.map(x => ({
            id: x.id || 0,
            name: x.name,
            price: x.price,
            qty: x.qty,
            stockQty: x.skipStock || x.custom ? 0 : (x.stockQty > 0 ? x.stockQty : 0),
            weightKg: x.weightKg || 0,
            skipStock: !!(x.skipStock || x.custom || !x.id)
        })),
        status,
        vatAmount,
        shippingAmount,
        discountAmount,
        totalAmount: t.total,
        shippingAddress: t.shipOn ? (opts.shippingAddress || '') : null
    };
    if (customerId) payload.customerId = customerId;
    if (opts.isPaid != null) payload.isPaid = opts.isPaid;
    else if (status === 'Completed') payload.isPaid = true;
    if (posShipping?.shippingTo) {
        payload.shippingAddress = posShipping.shippingTo;
        if (posShipping.deliveryDate) payload.deliveryDate = posShipping.deliveryDate;
        if (posShipping.dueDate) payload.dueDate = posShipping.dueDate;
        if (posShipping.customerId && !payload.customerId) payload.customerId = posShipping.customerId;
    } else if (t.shipOn) {
        payload.shippingAddress = opts.shippingAddress || '';
    }
    try {
        const res = await api('/api/checkout', { method: 'POST', body: JSON.stringify(payload) });
        const saleTotal = t.total;
        const custName = cust?.name || '';
        const priorBal = Number(cust?.balance) || 0;
        cart = [];
        posShipping = null;
        updateShippingButton();
        ['pos-vat', 'pos-ship', 'pos-disc'].forEach(id => { const el = document.getElementById(id); if (el) el.checked = false; });
        const shipAmt = document.getElementById('pos-ship-amt'); if (shipAmt) shipAmt.value = '0';
        const discAmt = document.getElementById('pos-disc-amt'); if (discAmt) discAmt.value = '0';
        clearPosTotalManual();
        renderCart();
        if (opts.isPaid === false) {
            toast(tr('bill_ok_detail')
                .replace('{0}', money(saleTotal))
                .replace('{1}', custName)
                .replace('{2}', money(priorBal + saleTotal)), 'success');
        }
        else if (status === 'Completed') toast(tr('checkout_ok'), 'success');
        else if (status === 'Quotation') toast(tr('quotation') + ' ✓', 'success');
        else toast(tr('save_draft') + ' ✓', 'success');
        await loadData();
        populatePosCustomer();
        renderPosStats();
        if (status === 'Quotation') {
            try { quotations = await api('/api/quotations'); renderQuotations(); } catch {}
        }
        return res;
    } catch (e) { toast(e.message || tr('checkout_fail'), 'error'); return null; }
}

function buildPosReceiptHtml() {
    const t = getPosTotals();
    const cust = document.getElementById('pos-customer');
    const custName = cust?.selectedOptions?.[0]?.textContent || tr('walk_in_customer');
    const lines = cart.length
        ? cart.map(x => `<tr><td>${escapeHtml(x.name)}</td><td>${x.qty}</td><td>${posMoney(x.price)}</td><td>${posMoney(x.price * x.qty)}</td></tr>`).join('')
        : `<tr><td colspan="4">${escapeHtml(tr('empty_cart_hint'))}</td></tr>`;
    return `
        <h2>${escapeHtml(tr('print_receipt'))}</h2>
        <p class="pos-rcpt-meta">${escapeHtml(new Date().toLocaleString())}</p>
        <p><strong>${escapeHtml(tr('col_customer'))}:</strong> ${escapeHtml(custName)}</p>
        ${posShipping?.shippingTo ? `<p><strong>${escapeHtml(tr('shipping_to'))}:</strong> ${escapeHtml(posShipping.shippingTo)}</p>` : ''}
        <table>
            <thead><tr><th>${tr('col_name')}</th><th>${tr('col_qty')}</th><th>${tr('col_price')}</th><th>${tr('col_total')}</th></tr></thead>
            <tbody>${lines}</tbody>
        </table>
        <div class="pos-rcpt-totals">
            <div><span>${tr('subtotal')}</span><span>${posMoney(t.sub)}</span></div>
            <div><span>${tr('vat_label')}</span><span>${posMoney(t.vat)}</span></div>
            <div><span>${tr('shipping')}</span><span>${posMoney(t.ship)}</span></div>
            <div><span>${tr('discount')}</span><span>${posMoney(t.disc)}</span></div>
            <div class="pos-rcpt-grand"><span>${tr('total_payable')}</span><span>${posMoney(t.total)}</span></div>
        </div>`;
}

function getPosPrintOptions() {
    const landscape = document.querySelector('input[name="ppd-layout"]:checked')?.value === 'landscape';
    const pagesMode = document.querySelector('input[name="ppd-pages"]:checked')?.value || 'all';
    const pageRange = pagesMode === 'custom'
        ? (document.getElementById('ppd-page-range')?.value || '').trim()
        : 'all';
    const copies = Math.max(1, Math.min(99, parseInt(document.getElementById('ppd-copies')?.value, 10) || 1));
    const color = (document.getElementById('ppd-color')?.value || 'color') !== 'bw';
    const printerName = document.getElementById('ppd-printer')?.value || '';
    const size = getLiveReceiptSize();
    return {
        landscape,
        pageRange,
        copies,
        color,
        printerName,
        paperWidthMm: size.paperWidthMm,
        paperHeightMm: size.paperHeightMm,
        marginMm: size.marginMm
    };
}

function applyPosPrintLayoutPreview() {
    const paper = document.getElementById('ppd-paper');
    if (!paper) return;
    const landscape = document.querySelector('input[name="ppd-layout"]:checked')?.value === 'landscape';
    paper.classList.toggle('is-landscape', landscape);
    paper.classList.add('is-receipt-roll');
    const size = getLiveReceiptSize();
    // ~3.8px per mm — preview is a narrow roll, not a giant A4 sheet
    const previewPx = Math.max(160, Math.min(380, Math.round(size.paperWidthMm * 3.8)));
    paper.style.width = `${previewPx}px`;
    paper.style.maxWidth = '100%';
    paper.style.minHeight = size.paperHeightMm > 0
        ? `${Math.round(size.paperHeightMm * 3.8)}px`
        : '280px';
    paper.style.aspectRatio = 'auto';
    paper.style.margin = '0 auto';
}

function openPosPrintPreview() {
    if (!cart.length) {
        toast(tr('empty_cart'), 'error');
        return;
    }
    const sheet = document.getElementById('pos-receipt-sheet');
    if (!sheet) return;
    sheet.innerHTML = buildPosReceiptHtml();
    const meta = document.getElementById('ppd-sheet-count');
    if (meta) meta.textContent = tr('print_sheet_one');
    fillPosPrintSizeControls();
    applyPosPrintLayoutPreview();
    requestHostPrinters();
    openModal('pos-print-modal');
    applyI18n();
}

function printPosReceipt() {
    openPosPrintPreview();
}

function doPrintPosReceipt() {
    if (!cart.length) {
        toast(tr('empty_cart'), 'error');
        return;
    }
    const opts = getPosPrintOptions();
    const t = getPosTotals();
    const cust = document.getElementById('pos-customer');
    const custName = cust?.selectedOptions?.[0]?.textContent || tr('walk_in_customer');
    const payload = {
        action: 'printReceipt',
        customerName: custName,
        shippingTo: posShipping?.shippingTo || '',
        currencyCode: posCurrencyMeta().code,
        currencySymbol: posCurrencyMeta().symbol,
        items: cart.map(x => ({
            name: x.name || '',
            qty: Number(x.qty) || 1,
            price: Number(x.price) || 0,
            total: (Number(x.price) || 0) * (Number(x.qty) || 1)
        })),
        subtotal: t.sub,
        vat: t.vat,
        shipping: t.ship,
        discount: t.disc,
        total: t.total,
        printerName: opts.printerName,
        copies: opts.copies,
        landscape: opts.landscape,
        color: opts.color,
        paperWidthMm: opts.paperWidthMm,
        paperHeightMm: opts.paperHeightMm,
        marginMm: opts.marginMm
    };

    if (window.chrome?.webview?.postMessage) {
        try {
            window.chrome.webview.postMessage(JSON.stringify(payload));
            closeModal('pos-print-modal');
            return;
        } catch (e) {
            toast(tr('print_failed'), 'error');
            return;
        }
    }

    // Browser fallback
    let box = document.getElementById('pos-print-receipt');
    if (!box) {
        box = document.createElement('div');
        box.id = 'pos-print-receipt';
        box.setAttribute('aria-hidden', 'true');
        document.body.appendChild(box);
    }
    box.innerHTML = buildPosReceiptHtml();
    const cleanup = () => {
        const el = document.getElementById('pos-print-receipt');
        if (el) { el.innerHTML = ''; el.remove(); }
        window.removeEventListener('afterprint', cleanup);
    };
    window.addEventListener('afterprint', cleanup);
    setTimeout(() => { if (!window.matchMedia('print').matches) cleanup(); }, 30000);
    closeModal('pos-print-modal');
    setTimeout(() => window.print(), 80);
}

function todayInputValue() {
    const d = new Date();
    return d.toISOString().slice(0, 10);
}

function openShippingModal() {
    const sel = document.getElementById('ship-customer');
    if (sel) {
        const cur = posShipping?.customerId || document.getElementById('pos-customer')?.value || '';
        sel.innerHTML = `<option value="">${escapeHtml(tr('walk_in_customer'))}</option>` +
            customers.map(c => `<option value="${c.id}">${escapeHtml(c.name)}</option>`).join('');
        if (cur) sel.value = String(cur);
    }
    document.getElementById('ship-to').value = posShipping?.shippingTo || '';
    document.getElementById('ship-order-date').value = posShipping?.orderDate || todayInputValue();
    document.getElementById('ship-delivery-date').value = posShipping?.deliveryDate || todayInputValue();
    document.getElementById('ship-due-date').value = posShipping?.dueDate || todayInputValue();
    openModal('shipping-modal');
}

async function loadDraftToCart(orderId, customerId) {
    try {
        const items = await api('/api/order-details/' + orderId);
        if (!items?.length) return toast(tr('empty_list'), 'error');
        if (cart.length && !await confirmDialog(tr('confirm_clear_cart'), { danger: false, confirmText: tr('confirm_btn') })) return;
        cart = items.map(it => {
            const p = products.find(x => x.id === it.partId) || { id: it.partId, name: it.name, price: it.price, stock: 9999, price2: 0, price3: 0, price4: 0 };
            return makeCartLine(p, Number(it.qty) || 1, Number(it.price));
        });
        const sel = document.getElementById('pos-customer');
        if (sel && customerId) sel.value = String(customerId);
        closeModal('drafts-modal');
        renderCart();
        toast(tr('draft_loaded'), 'success');
    } catch (e) { toast(e.message, 'error'); }
}

async function openDraftsModal() {
    openModal('drafts-modal');
    const body = document.getElementById('drafts-body');
    if (!body) return;
    body.innerHTML = `<tr><td colspan="5" class="empty-state">...</td></tr>`;
    try {
        const drafts = await api('/api/drafts');
        if (!drafts?.length) {
            body.innerHTML = `<tr><td colspan="5" class="empty-state">${tr('no_drafts')}</td></tr>`;
            return;
        }
        body.innerHTML = drafts.map(d => `
            <tr>
                <td>#${d.orderId}</td>
                <td>${formatDate(d.orderDate)}</td>
                <td>${escapeHtml(d.customerName || '')}</td>
                <td>${money(d.totalAmount)}</td>
                <td><div class="table-actions">
                    <button type="button" class="btn btn-primary btn-sm" data-load-draft="${d.orderId}" data-cust="${d.customerId || ''}">${tr('load_to_cart')}</button>
                    <button type="button" class="btn btn-secondary btn-sm" data-view-draft="${d.orderId}">${tr('view')}</button>
                    <button type="button" class="btn-icon btn-icon-danger" data-del-draft="${d.orderId}" title="${tr('delete')}"><span class="material-symbols-rounded">delete</span></button>
                </div></td>
            </tr>`).join('');
        body.querySelectorAll('[data-load-draft]').forEach(btn => {
            btn.onclick = () => loadDraftToCart(Number(btn.dataset.loadDraft), btn.dataset.cust ? Number(btn.dataset.cust) : null);
        });
        body.querySelectorAll('[data-view-draft]').forEach(btn => {
            btn.onclick = async () => {
                closeModal('drafts-modal');
                await viewOrder(Number(btn.dataset.viewDraft));
            };
        });
        body.querySelectorAll('[data-del-draft]').forEach(btn => {
            btn.onclick = async () => {
                if (!await confirmDialog(tr('confirm_delete'))) return;
                try {
                    await api('/api/quotations/' + btn.dataset.delDraft + '/delete', { method: 'POST' });
                    toast(tr('deleted_ok'), 'success');
                    openDraftsModal();
                    await loadData();
                    renderPosStats();
                } catch (e) { toast(e.message, 'error'); }
            };
        });
    } catch (e) {
        body.innerHTML = `<tr><td colspan="5" class="empty-state">${escapeHtml(e.message || tr('empty_list'))}</td></tr>`;
    }
}

function openBlindReturnModal() {
    blindReturnItems = [];
    const sel = document.getElementById('blind-customer');
    if (sel) {
        sel.innerHTML = `<option value="">${escapeHtml(tr('walk_in_customer'))}</option>` +
            customers.map(c => `<option value="${c.id}">${escapeHtml(c.name)}</option>`).join('');
    }
    const search = document.getElementById('blind-search');
    if (search) search.value = '';
    const reason = document.getElementById('blind-return-reason');
    if (reason) reason.value = '';
    renderBlindReturnItems();
    openModal('blind-return-modal');
    setTimeout(() => search?.focus(), 50);
}

function addBlindReturnProduct(p, qty = 1) {
    if (!p) return;
    const existing = blindReturnItems.find(x => x.partId === p.id);
    if (existing) existing.qty += qty;
    else blindReturnItems.push({ partId: p.id, name: p.name, qty, price: Number(p.price) || 0 });
    renderBlindReturnItems();
}

function renderBlindReturnItems() {
    const el = document.getElementById('blind-return-items');
    const totalEl = document.getElementById('blind-return-total');
    if (!el) return;
    if (!blindReturnItems.length) {
        el.innerHTML = `<div class="empty-state">${tr('empty_cart_hint')}</div>`;
        if (totalEl) totalEl.textContent = money(0);
        return;
    }
    el.innerHTML = blindReturnItems.map((it, i) => `
            <div class="blind-line">
            <strong>${escapeHtml(it.name)}</strong>
            <span class="blind-price-tag">${money(it.price)}</span>
            <input type="number" class="form-control" data-bq="${i}" min="1" step="1" value="${it.qty}" title="${tr('return_qty')}">
            <input type="number" class="form-control" data-bp="${i}" min="0.01" step="0.01" value="${Number(it.price).toFixed(2)}" title="${tr('unit_price')}">
            <button type="button" class="cart-remove" data-brm="${i}"><span class="material-symbols-rounded">delete</span></button>
        </div>`).join('');
    el.querySelectorAll('[data-bq]').forEach(inp => {
        inp.onchange = () => {
            const i = Number(inp.dataset.bq);
            blindReturnItems[i].qty = Math.max(1, parseInt(inp.value, 10) || 1);
            renderBlindReturnItems();
        };
    });
    el.querySelectorAll('[data-bp]').forEach(inp => {
        inp.onchange = () => {
            const i = Number(inp.dataset.bp);
            const v = parseFloat(inp.value);
            if (!(v > 0)) { toast(tr('invalid_price'), 'error'); renderBlindReturnItems(); return; }
            blindReturnItems[i].price = v;
            renderBlindReturnItems();
        };
    });
    el.querySelectorAll('[data-brm]').forEach(btn => {
        btn.onclick = () => { blindReturnItems.splice(Number(btn.dataset.brm), 1); renderBlindReturnItems(); };
    });
    const total = blindReturnItems.reduce((s, x) => s + x.price * x.qty, 0);
    if (totalEl) totalEl.textContent = money(total);
}

async function submitBlindReturn() {
    if (!blindReturnItems.length) return toast(tr('empty_cart_hint'), 'error');
    const customerId = document.getElementById('blind-customer')?.value;
    try {
        await api('/api/blind-return', {
            method: 'POST',
            body: JSON.stringify({
                reason: document.getElementById('blind-return-reason')?.value.trim() || 'Web blind return',
                customerId: customerId ? Number(customerId) : null,
                items: blindReturnItems.map(x => ({
                    partId: x.partId,
                    qty: x.qty,
                    refundAmount: x.price * x.qty
                }))
            })
        });
        closeModal('blind-return-modal');
        blindReturnItems = [];
        toast(tr('return_ok_blind'), 'success');
        await loadData();
        renderPosStats();
    } catch (e) { toast(e.message, 'error'); }
}

function openCustomerModal(id) {
    const title = document.getElementById('customer-modal-title');
    if (id) {
        const c = customers.find(x => x.id === id);
        if (!c) return;
        if (title) title.textContent = tr('edit');
        document.getElementById('cust-id').value = id;
        document.getElementById('cust-name').value = c.name || '';
        document.getElementById('cust-phone').value = c.phone || '';
        document.getElementById('cust-email').value = c.email || '';
        document.getElementById('cust-address').value = c.address || '';
        document.getElementById('cust-type').value = c.type || 'Regular';
        document.getElementById('cust-credit').value = c.creditLimit ?? 1000;
        document.getElementById('cust-due').value = c.dueDate ? c.dueDate.substring(0, 10) : '';
        document.getElementById('cust-reminder').value = c.reminderDays ?? 0;
    } else {
        if (title) title.textContent = tr('add_customer');
        document.getElementById('customer-form').reset();
        document.getElementById('cust-id').value = '';
        document.getElementById('cust-type').value = 'Regular';
        document.getElementById('cust-credit').value = 1000;
        document.getElementById('cust-reminder').value = 0;
    }
    openModal('customer-modal');
}

async function deleteCustomer(id) {
    if (!await confirmDialog(tr('confirm_delete'))) return;
    try {
        await api('/api/customers/' + id + '/delete', { method: 'POST' });
        toast(tr('deleted_ok'), 'success'); await loadData();
    } catch (e) { toast(e.message, 'error'); }
}

function openSupplierModal(id) {
    const title = document.getElementById('supplier-modal-title');
    if (id) {
        const s = suppliers.find(x => x.id === id);
        if (!s) return;
        if (title) title.textContent = tr('edit');
        document.getElementById('supp-id').value = id;
        document.getElementById('supp-name').value = s.name || '';
        document.getElementById('supp-phone').value = s.phone || '';
        document.getElementById('supp-email').value = s.email || '';
        document.getElementById('supp-address').value = s.address || '';
        document.getElementById('supp-contact').value = s.contact || '';
        document.getElementById('supp-type').value = s.type || 'Regular';
        document.getElementById('supp-due').value = s.dueDate ? s.dueDate.substring(0, 10) : '';
        document.getElementById('supp-reminder').value = s.reminderDays ?? 0;
    } else {
        if (title) title.textContent = tr('add_supplier');
        document.getElementById('supplier-form').reset();
        document.getElementById('supp-id').value = '';
        document.getElementById('supp-type').value = 'Regular';
        document.getElementById('supp-reminder').value = 0;
    }
    openModal('supplier-modal');
}

async function deleteSupplier(id) {
    if (!await confirmDialog(tr('confirm_delete'))) return;
    try {
        await api('/api/suppliers/' + id + '/delete', { method: 'POST' });
        toast(tr('deleted_ok'), 'success'); await loadData();
    } catch (e) { toast(e.message, 'error'); }
}

function openUserModal(id) {
    const title = document.getElementById('user-modal-title');
    const pass = document.getElementById('user-password');
    if (id) {
        const u = users.find(x => x.id === id);
        if (!u) return;
        if (title) title.textContent = tr('edit');
        document.getElementById('user-id').value = id;
        document.getElementById('user-username').value = u.username || '';
        document.getElementById('user-fullname').value = u.fullName || '';
        document.getElementById('user-role').value = u.role || 'Staff';
        if (pass) { pass.value = ''; pass.required = false; }
    } else {
        if (title) title.textContent = tr('add_user');
        document.getElementById('user-form').reset();
        document.getElementById('user-id').value = '';
        document.getElementById('user-role').value = 'Staff';
        if (pass) pass.required = true;
    }
    openModal('user-modal');
}

async function deleteUser(id) {
    const u = users.find(x => x.id === id);
    if (u && isProtectedSuperAdmin(u)) {
        toast(tr('cannot_delete_super_admin'), 'error');
        return;
    }
    if (!await confirmDialog(tr('confirm_delete'))) return;
    try {
        await api('/api/users/' + id + '/delete', { method: 'POST' });
        toast(tr('deleted_ok'), 'success'); await loadData();
    } catch (e) { toast(e.message, 'error'); }
}

async function deleteQuotation(id) {
    if (!await confirmDialog(tr('confirm_delete'))) return;
    try {
        await api('/api/quotations/' + id + '/delete', { method: 'POST' });
        toast(tr('deleted_ok'), 'success'); await loadData();
    } catch (e) { toast(e.message, 'error'); }
}

async function deleteExpense(id) {
    if (!await confirmDialog(tr('confirm_delete'))) return;
    try {
        await api('/api/expenses/' + id + '/delete', { method: 'POST' });
        toast(tr('deleted_ok'), 'success'); await loadData();
    } catch (e) { toast(e.message, 'error'); }
}

async function deleteCurrency(code) {
    if (!await confirmDialog(tr('confirm_delete'))) return;
    try {
        await api('/api/currencies/' + encodeURIComponent(code) + '/delete', { method: 'POST' });
        toast(tr('deleted_ok'), 'success'); await loadData();
    } catch (e) { toast(e.message, 'error'); }
}

async function viewOrder(orderId) {
    try {
        const items = await api('/api/order-details/' + orderId);
        const body = document.getElementById('order-view-body');
        if (!items?.length) {
            body.innerHTML = `<div class="empty-state">${tr('empty_list')}</div>`;
        } else {
            body.innerHTML = `<table><thead><tr>
                <th>${tr('col_name')}</th><th>${tr('col_qty')}</th><th>${tr('col_price')}</th><th>${tr('total')}</th>
            </tr></thead><tbody>${items.map(it => `<tr>
                <td>${escapeHtml(it.name)}</td><td>${it.qty}</td><td>${money(it.price)}</td><td>${money(it.price * it.qty)}</td>
            </tr>`).join('')}</tbody></table>`;
        }
        openModal('order-view-modal');
    } catch (e) { toast(e.message, 'error'); }
}

async function importCustomersCsv(file) {
    const { headers, rows } = await parseCsvFile(file);
    if (!rows.length) return toast(tr('empty_list'), 'error');
    try {
        const res = await api('/api/customers/import', { method: 'POST', body: JSON.stringify({ rows, headers }) });
        toastImportResult(res);
        await loadData();
    } catch (e) { toast(e.message, 'error'); }
}

async function importSuppliersCsv(file) {
    const { headers, rows } = await parseCsvFile(file);
    if (!rows.length) return toast(tr('empty_list'), 'error');
    try {
        const res = await api('/api/suppliers/import', { method: 'POST', body: JSON.stringify({ rows, headers }) });
        toastImportResult(res);
        await loadData();
    } catch (e) { toast(e.message, 'error'); }
}

async function importInventoryCsv(file) {
    const { headers, rows } = await parseCsvFile(file);
    if (!rows.length) return toast(tr('empty_list'), 'error');
    try {
        const res = await api('/api/products/import', { method: 'POST', body: JSON.stringify({ rows, headers }) });
        toastImportResult(res);
        await loadData();
    } catch (e) { toast(e.message, 'error'); }
}

async function importSalesCsv(file) {
    const { rows } = await parseCsvFile(file);
    if (!rows.length) return toast(tr('empty_list'), 'error');
    const payload = rows.map(row => ({
        customer: csvCell(row, 'customer', 'customer_name'),
        total: parseFloat(csvCell(row, 'total', 'amount', 'total_amount') || '0') || 0,
        date: csvCell(row, 'date', 'order_date'),
        payment: csvCell(row, 'payment', 'paymentstatus', 'payment_status', 'status') || 'Paid'
    })).filter(r => r.total > 0);
    if (!payload.length) return toast(tr('empty_list'), 'error');
    try {
        const res = await api('/api/sales/import', { method: 'POST', body: JSON.stringify(payload) });
        toast(tr('import_ok') + ' (' + (res.imported || 0) + ')', 'success');
        await loadData();
    } catch (e) { toast(e.message, 'error'); }
}

async function importHistoryCsv(file) {
    const { rows } = await parseCsvFile(file);
    if (!rows.length) return toast(tr('empty_list'), 'error');
    const payload = rows.map(row => ({
        date: csvCell(row, 'date'),
        action: csvCell(row, 'action', 'type'),
        item: csvCell(row, 'item', 'name', 'sku'),
        customer: csvCell(row, 'customer'),
        details: csvCell(row, 'details', 'description', 'desc'),
        description: csvCell(row, 'description', 'details', 'desc'),
        user: csvCell(row, 'user', 'username'),
        status: csvCell(row, 'status'),
        payment: csvCell(row, 'payment', 'paymentstatus', 'payment_status'),
        total: parseFloat(csvCell(row, 'total', 'amount') || '0') || 0
    }));
    try {
        const res = await api('/api/history/import', { method: 'POST', body: JSON.stringify(payload) });
        toast(tr('import_ok') + ' (' + (res.imported || 0) + ')', 'success');
        await loadData();
        await loadHistory();
        renderHistory();
    } catch (e) { toast(e.message, 'error'); }
}

async function importReportsCsv(file) {
    const { rows } = await parseCsvFile(file);
    if (!rows.length) return toast(tr('empty_list'), 'error');
    const payload = rows.map(row => ({
        rowType: csvCell(row, 'row_type', 'rowtype') || (csvCell(row, 'name', 'product') ? 'product' : 'summary'),
        name: csvCell(row, 'name', 'product', 'product_name'),
        qty: parseFloat(csvCell(row, 'qty', 'quantity') || '0') || 0,
        sales: parseFloat(csvCell(row, 'sales', 'total_sales') || '0') || 0,
        profit: parseFloat(csvCell(row, 'profit') || '0') || 0,
        total: parseFloat(csvCell(row, 'total', 'amount') || '0') || 0,
        date: csvCell(row, 'date'),
        metric: csvCell(row, 'metric'),
        value: csvCell(row, 'value')
    }));
    try {
        const res = await api('/api/reports/import', { method: 'POST', body: JSON.stringify(payload) });
        toast(tr('import_ok') + ' (' + (res.imported || 0) + ')', 'success');
        await loadData();
        await loadReports();
        renderReports();
    } catch (e) { toast(e.message, 'error'); }
}

async function importExpensesCsv(file) {
    const { rows } = await parseCsvFile(file);
    if (!rows.length) return toast(tr('empty_list'), 'error');
    try {
        const res = await api('/api/expenses/import', {
            method: 'POST',
            body: JSON.stringify({ rows, recordedBy: currentUser?.username || 'Web' })
        });
        toastImportResult(res);
        await loadData();
    } catch (e) { toast(e.message, 'error'); }
}

function setupNavigation() {
    document.querySelectorAll('.nav-menu .nav-item[data-target]').forEach(item => {
        item.addEventListener('click', async e => {
            e.preventDefault();
            const target = item.getAttribute('data-target');
            if (!target || item.style.display === 'none') return;
            await navigateTo(target, true);
        });
    });
    window.addEventListener('hashchange', () => {
        if (!currentUser) return;
        const target = (location.hash || '').replace(/^#\/?/, '');
        if (target) navigateTo(target, false);
    });
}

async function navigateTo(target, updateHash = true) {
    const item = document.querySelector(`.nav-menu .nav-item[data-target="${target}"]`);
    if (!item || item.style.display === 'none') return;
    if (!document.getElementById(target)) return;

    document.querySelectorAll('.nav-menu .nav-item').forEach(n => n.classList.remove('active'));
    document.querySelectorAll('.view-section').forEach(s => s.classList.remove('active'));
    item.classList.add('active');
    document.getElementById(target).classList.add('active');
    if (updateHash) {
        const next = '#/' + target;
        if (location.hash !== next) history.replaceState(null, '', next);
    }
    if (target === 'reports') { await loadReports(); renderReports(); }
    if (target === 'history') { await loadHistory(); renderHistory(); }
    if (target === 'inventory') {
        requestAnimationFrame(() => {
            if (typeof updateInvActionsSlide === 'function') updateInvActionsSlide();
        });
        setTimeout(() => {
            if (typeof updateInvActionsSlide === 'function') updateInvActionsSlide();
        }, 80);
    }
    if (target === 'settings') {
        await loadLicense();
        renderLicense();
        await loadFeatureFlags();
        await loadPrintSettings();
        updateSettingsLangLabel();
        requestHostPrinters();
    }
    if (target === 'barcodes') renderBarcodes();
    if (target === 'pos') {
        setTimeout(() => document.getElementById('pos-search')?.focus(), 50);
    }
}

function setupAuth() {
    document.getElementById('login-form').addEventListener('submit', async e => {
        e.preventDefault();
        const err = document.getElementById('login-error');
        err.classList.remove('visible');
        const btn = e.target.querySelector('button[type="submit"]');
        if (btn) btn.disabled = true;
        try {
            const user = await api('/api/login', {
                method: 'POST',
                body: JSON.stringify({
                    username: document.getElementById('login-user').value.trim(),
                    password: document.getElementById('login-pass').value
                })
            });
            currentUser = user;
            sessionStorage.setItem('generic_user', JSON.stringify(user));
            applyFeatureFlags(user.features || { scaleEnabled: false, quickSaleEnabled: false });
            document.getElementById('login-pass').value = '';
            document.getElementById('login-user').value = '';
            showApp();
            applyI18n();
            try {
                await loadFeatureFlags();
                await loadData();
            } catch (loadErr) {
                console.error('loadData failed', loadErr);
                toast(loadErr.message || 'Data load failed', 'error');
            }
        } catch {
            hideApp();
            err.textContent = tr('login_failed');
            err.classList.add('visible');
        } finally {
            if (btn) btn.disabled = false;
        }
    });
    document.getElementById('btn-logout').onclick = () => hideApp();
}

function setupActions() {
    const btnLang = document.getElementById('btn-lang');
    if (btnLang) btnLang.onclick = () => toggleLanguage();
    document.getElementById('btn-settings-lang')?.addEventListener('click', () => toggleLanguage());

    document.getElementById('btn-copy-connect-url')?.addEventListener('click', async () => {
        const url = document.getElementById('dash-connect-url')?.textContent?.trim();
        if (!url || url === '—') return;
        try {
            await navigator.clipboard.writeText(url);
            toast(tr('copied'), 'success');
        } catch {
            try {
                const ta = document.createElement('textarea');
                ta.value = url;
                document.body.appendChild(ta);
                ta.select();
                document.execCommand('copy');
                ta.remove();
                toast(tr('copied'), 'success');
            } catch { toast(url); }
        }
    });

    const btnAddProduct = document.getElementById('btn-add-product');
    if (btnAddProduct) btnAddProduct.onclick = () => openProductModal(null);
    document.getElementById('product-form').onsubmit = async e => {
        e.preventDefault();
        const payload = buildProductPayload();
        const pid = document.getElementById('p-id').value;
        try {
            let res;
            if (pid) {
                res = await api('/api/products/' + pid + '/update', { method: 'POST', body: JSON.stringify(payload) });
            } else {
                res = await api('/api/add-item', { method: 'POST', body: JSON.stringify(payload) });
            }
            if (res?.barcode) payload.barcode = res.barcode;
            const inFsQueue = !!(fsImportQueue && fsImportQueue.awaitingSave);
            if (inFsQueue) fsImportQueue.awaitingSave = false; // prevent cancel handler on close
            closeModal('product-modal'); editingProductId = null;
            suppressSignalRReload(2000);
            if (!pid) insertOptimisticProduct(payload);
            toast(tr(pid ? 'saved_ok' : 'product_ok'), 'success');
            await loadData();
            if (inFsQueue) {
                onFsImportProductSaved();
            } else if (document.getElementById('from-supplier-modal')?.classList.contains('active')) {
                await loadFromSupplierList();
            }
        } catch (err) { toast(err.message, 'error'); }
    };

    document.getElementById('btn-p-auto-sku')?.addEventListener('click', generateAutoSku);
    document.getElementById('btn-p-scan')?.addEventListener('click', () => {
        toast(tr('scan'), 'success');
        document.getElementById('p-barcode')?.focus();
    });
    document.getElementById('btn-p-upload')?.addEventListener('click', () => document.getElementById('p-image-file')?.click());
    document.getElementById('btn-p-change-image')?.addEventListener('click', () => document.getElementById('p-image-file')?.click());
    document.getElementById('btn-p-clear-image')?.addEventListener('click', () => clearProductImage());
    document.getElementById('btn-p-remove-image')?.addEventListener('click', () => clearProductImage());
    document.getElementById('p-image-preview')?.addEventListener('click', () => {
        if (document.getElementById('p-image')?.value) document.getElementById('p-image-file')?.click();
    });
    document.getElementById('p-image-file')?.addEventListener('change', async e => {
        const file = e.target.files?.[0];
        if (!file) return;
        try {
            const res = await uploadProductImage(file);
            setProductImagePreview(res.path || res.url || '');
            toast(tr('saved_ok'), 'success');
        } catch (err) { toast(err.message, 'error'); }
        e.target.value = '';
    });
    document.querySelectorAll('input[name="p-type"]').forEach(r => r.addEventListener('change', () => {
        onProductTypeChange();
        if (!document.getElementById('p-id').value) {
            const title = document.getElementById('product-modal-title');
            if (title) title.textContent = tr(getProductTypeValue() === 'Service' ? 'add_service' : 'add_product');
        }
    }));
    document.getElementById('p-track-stock')?.addEventListener('change', onProductTypeChange);
    document.querySelectorAll('input[name="p-sell-by"]').forEach(r => r.addEventListener('change', () => updateSellByWeightUI()));
    document.getElementById('btn-p-add-uom')?.addEventListener('click', () => { addCustomUom(); });
    document.getElementById('p-uom')?.addEventListener('change', () => {
        if (getSellByValue() !== 'weight') updatePieceStockLabels();
    });
    ['p-cost', 'p-price', 'p-price2', 'p-price3', 'p-price4'].forEach(id => {
        document.getElementById(id)?.addEventListener('input', calculateProductMargins);
    });

    document.getElementById('btn-add-category')?.addEventListener('click', () => {
        document.getElementById('category-form').reset();
        document.getElementById('cat-old-name').value = '';
        openModal('category-modal');
    });
    document.getElementById('btn-manage-categories')?.addEventListener('click', () => {
        renderCategoriesManage();
        openModal('categories-manage-modal');
    });
    document.getElementById('category-form')?.addEventListener('submit', async e => {
        e.preventDefault();
        const oldName = document.getElementById('cat-old-name').value.trim();
        const newName = document.getElementById('cat-name').value.trim();
        try {
            if (oldName) {
                await api('/api/categories/rename', { method: 'POST', body: JSON.stringify({ oldName, newName }) });
            } else {
                await api('/api/categories', { method: 'POST', body: JSON.stringify({ name: newName }) });
            }
            closeModal('category-modal');
            document.getElementById('cat-old-name').value = '';
            toast(tr('saved_ok'), 'success');
            await loadData();
        } catch (err) { toast(err.message, 'error'); }
    });

    document.getElementById('stock-form')?.addEventListener('submit', submitStockAdjust);

    document.getElementById('customer-payment-form')?.addEventListener('submit', async e => {
        e.preventDefault();
        await submitCustomerDebtPayment();
    });
    document.getElementById('btn-debt-pay')?.addEventListener('click', () => submitCustomerDebtPayment());
    document.getElementById('btn-debt-select-none')?.addEventListener('click', () => {
        const root = document.getElementById('customer-debt-orders');
        if (!root) return;
        root.querySelectorAll('input[type="checkbox"]').forEach(c => { c.checked = false; });
        root.querySelectorAll('.debt-order-pay, .debt-item-pay').forEach(i => { i.value = ''; });
        const amt = document.getElementById('cust-pay-amount');
        if (amt) amt.value = '';
        updateDebtSelectedTotal();
    });

    document.getElementById('supplier-payment-form')?.addEventListener('submit', async e => {
        e.preventDefault();
        const id = document.getElementById('supp-pay-id').value;
        const amount = Number(document.getElementById('supp-pay-amount').value);
        const note = document.getElementById('supp-pay-note').value.trim();
        try {
            await api('/api/suppliers/' + id + '/payment', { method: 'POST', body: JSON.stringify({ amount, note }) });
            closeModal('supplier-details-modal');
            toast(tr('payment_ok'), 'success');
            await loadData();
        } catch (err) { toast(err.message, 'error'); }
    });

    document.getElementById('btn-sp-add')?.addEventListener('click', () => addSupplierPurchaseLine());
    document.getElementById('p-supplier')?.addEventListener('change', () => {
        const link = document.getElementById('p-supplier-purchase-id');
        if (link) link.value = '';
        refreshProductSupplierPurchases();
    });
    document.getElementById('btn-from-supplier')?.addEventListener('click', () => openFromSupplierModal());
    document.getElementById('fs-supplier')?.addEventListener('change', () => loadFromSupplierList());
    document.getElementById('fs-select-all')?.addEventListener('change', e => {
        document.querySelectorAll('#fs-list [data-fs-check]').forEach(cb => { cb.checked = e.target.checked; });
    });
    document.getElementById('btn-fs-import-selected')?.addEventListener('click', () => importSelectedFromSupplier());
    initInvActionsSlide();

    document.getElementById('btn-manage-exp-cats')?.addEventListener('click', async () => {
        try { expenseCategories = await api('/api/expense-categories'); } catch { expenseCategories = []; }
        renderExpenseCategoriesList();
        openModal('exp-cats-modal');
    });
    document.getElementById('exp-cat-add-form')?.addEventListener('submit', async e => {
        e.preventDefault();
        const name = document.getElementById('exp-cat-new-name').value.trim();
        if (!name) return;
        try {
            await api('/api/expense-categories', { method: 'POST', body: JSON.stringify({ name }) });
            document.getElementById('exp-cat-new-name').value = '';
            expenseCategories = await api('/api/expense-categories');
            renderExpenseCategoriesList();
            toast(tr('saved_ok'), 'success');
        } catch (err) { toast(err.message, 'error'); }
    });

    document.getElementById('btn-add-customer')?.addEventListener('click', () => openCustomerModal(null));
    document.getElementById('customer-form')?.addEventListener('submit', async e => {
        e.preventDefault();
        const payload = {
            name: document.getElementById('cust-name').value.trim(),
            phone: document.getElementById('cust-phone').value.trim(),
            email: document.getElementById('cust-email').value.trim(),
            address: document.getElementById('cust-address').value.trim(),
            type: document.getElementById('cust-type').value.trim() || 'Regular',
            creditLimit: Number(document.getElementById('cust-credit')?.value) || 1000,
            dueDate: document.getElementById('cust-due')?.value || null,
            reminderDays: Number(document.getElementById('cust-reminder')?.value) || 0
        };
        const cid = document.getElementById('cust-id').value;
        try {
            if (cid) await api('/api/customers/' + cid + '/update', { method: 'POST', body: JSON.stringify(payload) });
            else await api('/api/customers', { method: 'POST', body: JSON.stringify(payload) });
            closeModal('customer-modal'); toast(tr('saved_ok'), 'success'); await loadData();
            populatePosCustomer();
            // Select newly added customer when created from POS
            if (!cid) {
                const sorted = [...customers].sort((a, b) => (b.id || 0) - (a.id || 0));
                if (sorted[0]) {
                    const sel = document.getElementById('pos-customer');
                    if (sel) sel.value = String(sorted[0].id);
                }
            }
        } catch (err) { toast(err.message, 'error'); }
    });
    document.getElementById('btn-import-inventory')?.addEventListener('click', () => document.getElementById('inv-import-file')?.click());
    document.getElementById('inv-import-file')?.addEventListener('change', async e => {
        const f = e.target.files?.[0];
        if (f) await importInventoryCsv(f);
        e.target.value = '';
    });
    document.getElementById('btn-import-customers')?.addEventListener('click', () => document.getElementById('cust-import-file')?.click());
    document.getElementById('cust-import-file')?.addEventListener('change', async e => {
        const file = e.target.files?.[0];
        if (file) await importCustomersCsv(file);
        e.target.value = '';
    });
    document.getElementById('btn-import-suppliers')?.addEventListener('click', () => document.getElementById('supp-import-file')?.click());
    document.getElementById('supp-import-file')?.addEventListener('change', async e => {
        const file = e.target.files?.[0];
        if (file) await importSuppliersCsv(file);
        e.target.value = '';
    });
    document.getElementById('btn-import-sales')?.addEventListener('click', () => document.getElementById('sales-import-file')?.click());
    document.getElementById('sales-import-file')?.addEventListener('change', async e => {
        const file = e.target.files?.[0];
        if (file) await importSalesCsv(file);
        e.target.value = '';
    });
    document.getElementById('btn-import-history')?.addEventListener('click', () => document.getElementById('history-import-file')?.click());
    document.getElementById('history-import-file')?.addEventListener('change', async e => {
        const file = e.target.files?.[0];
        if (file) await importHistoryCsv(file);
        e.target.value = '';
    });
    document.getElementById('btn-import-reports')?.addEventListener('click', () => document.getElementById('reports-import-file')?.click());
    document.getElementById('reports-import-file')?.addEventListener('change', async e => {
        const file = e.target.files?.[0];
        if (file) await importReportsCsv(file);
        e.target.value = '';
    });
    document.getElementById('btn-import-expenses')?.addEventListener('click', () => document.getElementById('exp-import-file')?.click());
    document.getElementById('exp-import-file')?.addEventListener('change', async e => {
        const file = e.target.files?.[0];
        if (file) await importExpensesCsv(file);
        e.target.value = '';
    });

    document.getElementById('btn-add-supplier')?.addEventListener('click', () => openSupplierModal(null));
    document.getElementById('supplier-form')?.addEventListener('submit', async e => {
        e.preventDefault();
        const contact = document.getElementById('supp-contact').value.trim();
        const payload = {
            name: document.getElementById('supp-name').value.trim(),
            phone: document.getElementById('supp-phone').value.trim(),
            email: document.getElementById('supp-email').value.trim(),
            address: document.getElementById('supp-address').value.trim(),
            contact,
            type: document.getElementById('supp-type')?.value.trim() || contact || 'Regular',
            dueDate: document.getElementById('supp-due')?.value || null,
            reminderDays: Number(document.getElementById('supp-reminder')?.value) || 0
        };
        const sid = document.getElementById('supp-id').value;
        try {
            if (sid) await api('/api/suppliers/' + sid + '/update', { method: 'POST', body: JSON.stringify(payload) });
            else await api('/api/suppliers', { method: 'POST', body: JSON.stringify(payload) });
            closeModal('supplier-modal'); toast(tr('saved_ok'), 'success'); await loadData();
        } catch (err) { toast(err.message, 'error'); }
    });

    document.getElementById('btn-add-user')?.addEventListener('click', () => openUserModal(null));
    document.getElementById('user-form')?.addEventListener('submit', async e => {
        e.preventDefault();
        const payload = {
            username: document.getElementById('user-username').value.trim(),
            fullName: document.getElementById('user-fullname').value.trim(),
            password: document.getElementById('user-password').value,
            role: document.getElementById('user-role').value
        };
        const uid = document.getElementById('user-id').value;
        try {
            if (uid) {
                if (!payload.password) delete payload.password;
                await api('/api/users/' + uid + '/update', { method: 'POST', body: JSON.stringify(payload) });
            } else {
                if (!payload.password) return toast(tr('login_failed'), 'error');
                await api('/api/users', { method: 'POST', body: JSON.stringify(payload) });
            }
            closeModal('user-modal'); toast(tr('saved_ok'), 'success'); await loadData();
        } catch (err) { toast(err.message, 'error'); }
    });

    document.getElementById('btn-add-currency')?.addEventListener('click', () => {
        document.getElementById('currency-form').reset();
        document.getElementById('cur-rate').value = '1';
        openModal('currency-modal');
    });
    document.getElementById('currency-form')?.addEventListener('submit', async e => {
        e.preventDefault();
        try {
            await api('/api/currencies', { method: 'POST', body: JSON.stringify({
                code: document.getElementById('cur-code').value.trim().toUpperCase(),
                name: document.getElementById('cur-name').value.trim(),
                symbol: document.getElementById('cur-symbol').value.trim(),
                rate: Number(document.getElementById('cur-rate').value)
            })});
            closeModal('currency-modal'); toast(tr('saved_ok'), 'success'); await loadData();
        } catch (err) { toast(err.message, 'error'); }
    });
    document.getElementById('btn-refresh-rates')?.addEventListener('click', async () => {
        try {
            await api('/api/currencies/refresh', { method: 'POST', body: '{}' });
            toast(tr('saved_ok'), 'success'); await loadData();
        } catch (err) { toast(err.message, 'error'); }
    });

    function populateExpenseSuppliers(selectedId) {
        const suppSel = document.getElementById('e-supplier');
        if (!suppSel) return;
        const cur = selectedId !== undefined ? String(selectedId) : suppSel.value;
        const list = suppliers || [];
        suppSel.innerHTML = `<option value="">— ${escapeHtml(tr('col_deliverer'))} —</option>` +
            `<option value="__add_new_deliverer__" style="font-weight:700; color:var(--primary);">${escapeHtml(tr('add_new_deliverer'))}</option>` +
            list.map(s => `<option value="${s.id}">${escapeHtml(s.name)}${s.phone ? ` (${escapeHtml(s.phone)})` : ''}</option>`).join('');
        if (cur && [...suppSel.options].some(o => o.value === cur)) suppSel.value = cur;
    }

    async function promptAddNewDeliverer() {
        const suppSel = document.getElementById('e-supplier');
        const name = await promptDialog({
            title: tr('add_new_deliverer'),
            message: tr('quick_add_deliverer_prompt'),
            confirmText: tr('save'),
            cancelText: tr('cancel'),
            placeholder: tr('col_deliverer')
        });
        if (!name || !name.trim()) {
            if (suppSel && suppSel.value === '__add_new_deliverer__') suppSel.value = '';
            return null;
        }
        try {
            await api('/api/suppliers', {
                method: 'POST',
                body: JSON.stringify({
                    name: name.trim(),
                    phone: '',
                    type: 'Regular'
                })
            });
            suppliers = await api('/api/suppliers').catch(() => []);
            const clean = name.trim().toLowerCase();
            const created = (suppliers || []).find(s => (s.name || '').trim().toLowerCase() === clean) || suppliers[suppliers.length - 1];
            populateExpenseSuppliers(created ? created.id : '');
            toast(tr('deliverer_added_ok'), 'success');
            return created ? created.id : null;
        } catch (err) {
            toast(err.message, 'error');
            populateExpenseSuppliers();
            return null;
        }
    }

    document.getElementById('btn-quick-add-supplier')?.addEventListener('click', promptAddNewDeliverer);
    document.getElementById('e-supplier')?.addEventListener('change', async e => {
        if (e.target.value === '__add_new_deliverer__') {
            await promptAddNewDeliverer();
        }
    });

    function toggleExpenseMilkFields(isMilk) {
        const milkDiv = document.getElementById('e-milk-fields');
        const recWrap = document.getElementById('e-recurring-wrap');
        if (milkDiv) milkDiv.style.display = isMilk ? '' : 'none';
        if (recWrap) recWrap.style.display = isMilk ? 'none' : '';
        populateExpenseSuppliers();
    }

    const calcMilkAmount = () => {
        const lit = parseFloat(document.getElementById('e-liters')?.value) || 0;
        const prc = parseFloat(document.getElementById('e-unit-price')?.value) || 0;
        if (lit > 0 && prc > 0) {
            document.getElementById('e-amount').value = (lit * prc).toFixed(2);
        }
    };
    document.getElementById('e-liters')?.addEventListener('input', calcMilkAmount);
    document.getElementById('e-unit-price')?.addEventListener('input', calcMilkAmount);

    document.getElementById('btn-add-milk-intake')?.addEventListener('click', async () => {
        await fillExpenseCategorySelect();
        const catSel = document.getElementById('e-category');
        if (catSel) {
            let found = [...catSel.options].find(o => o.value === 'Raw Milk' || o.text === 'Raw Milk' || o.value.includes('حليب') || o.text.includes('حليب'));
            if (!found) {
                const opt = document.createElement('option');
                opt.value = 'Raw Milk';
                opt.textContent = 'Raw Milk';
                catSel.insertBefore(opt, catSel.firstChild);
                catSel.value = 'Raw Milk';
            } else {
                catSel.value = found.value;
            }
        }
        document.getElementById('expense-form').reset();
        const titleEl = document.getElementById('expense-modal-title');
        if (titleEl) titleEl.textContent = tr('add_milk_intake');
        document.getElementById('e-date').value = formatLocalDate(new Date());
        const payStatus = document.getElementById('e-payment-status');
        if (payStatus) payStatus.value = 'unpaid';
        toggleExpenseMilkFields(true);
        openModal('expense-modal');
    });

    document.getElementById('btn-add-expense').onclick = async () => {
        await fillExpenseCategorySelect();
        document.getElementById('expense-form').reset();
        const titleEl = document.getElementById('expense-modal-title');
        if (titleEl) titleEl.textContent = tr('add_expense');
        document.getElementById('e-date').value = formatLocalDate(new Date());
        const payStatus = document.getElementById('e-payment-status');
        if (payStatus) payStatus.value = 'paid';
        const catSel = document.getElementById('e-category');
        const isMilk = catSel?.value === 'Raw Milk' || catSel?.value.includes('حليب');
        toggleExpenseMilkFields(isMilk);
        openModal('expense-modal');
    };

    document.getElementById('e-category')?.addEventListener('change', async e => {
        const select = e.target;
        if (select.value === '__add_new__') {
            const name = await promptDialog({
                title: tr('add_exp_category'),
                message: tr('add_exp_category'),
                confirmText: tr('save'),
                cancelText: tr('cancel'),
                placeholder: tr('add_exp_category')
            });
            if (!name) {
                await fillExpenseCategorySelect();
                return;
            }
            try {
                await api('/api/expense-categories', { method: 'POST', body: JSON.stringify({ name }) });
                expenseCategories = await api('/api/expense-categories').catch(() => []);
                await fillExpenseCategorySelect();
                const clean = String(name).trim();
                if (clean) select.value = clean;
                toast(tr('saved_ok'), 'success');
            } catch (err) {
                await fillExpenseCategorySelect();
                toast(err.message, 'error');
            }
        }
        const isMilk = select.value === 'Raw Milk' || select.value.includes('حليب');
        if (isMilk) {
            const payStatus = document.getElementById('e-payment-status');
            if (payStatus && payStatus.value === 'paid') payStatus.value = 'unpaid';
        }
        toggleExpenseMilkFields(isMilk);
    });

    document.getElementById('expense-form').onsubmit = async e => {
        e.preventDefault();
        try {
            const cat = document.getElementById('e-category').value;
            const isMilk = cat === 'Raw Milk' || cat.includes('حليب');
            const suppVal = document.getElementById('e-supplier')?.value;
            const suppId = (suppVal && suppVal !== '__add_new_deliverer__') ? (Number(suppVal) || null) : null;
            const liters = isMilk ? (Number(document.getElementById('e-liters')?.value) || 0) : 0;
            const unitPrice = isMilk ? (Number(document.getElementById('e-unit-price')?.value) || 0) : 0;
            const milkType = isMilk ? (document.getElementById('e-milk-type')?.value || 'Cow Milk') : null;
            const shift = isMilk ? (document.getElementById('e-shift')?.value || 'Morning') : null;
            const paymentStatus = document.getElementById('e-payment-status')?.value || 'paid';
            const isPaid = (paymentStatus === 'paid');

            await api('/api/expenses', { method: 'POST', body: JSON.stringify({
                category: cat,
                amount: Number(document.getElementById('e-amount').value),
                expenseDate: document.getElementById('e-date').value,
                description: document.getElementById('e-desc').value,
                recordedBy: currentUser?.username || 'Web',
                isRecurring: isMilk ? false : !!document.getElementById('e-recurring')?.checked,
                isPaid: isPaid,
                supplierId: suppId,
                liters: liters,
                unitPrice: unitPrice,
                milkType: milkType,
                shift: shift
            })});
            closeModal('expense-modal'); toast(tr('expense_ok'), 'success'); await loadData();
        } catch (err) { toast(err.message, 'error'); }
    };

    // Filter controls for Expenses & Milk Intake
    document.getElementById('exp-filter-preset')?.addEventListener('change', e => {
        syncExpenseDatePreset(e.target.value);
        renderExpenses();
    });
    document.getElementById('exp-filter-type')?.addEventListener('change', () => renderExpenses());
    document.getElementById('exp-filter-supplier')?.addEventListener('change', () => renderExpenses());
    document.getElementById('exp-filter-status')?.addEventListener('change', () => renderExpenses());
    document.getElementById('exp-filter-from')?.addEventListener('input', () => renderExpenses());
    document.getElementById('exp-filter-to')?.addEventListener('input', () => renderExpenses());
    document.getElementById('btn-settle-deliverer')?.addEventListener('click', () => settleSelectedDeliverer());
    document.getElementById('btn-print-milk-statement')?.addEventListener('click', () => {
        document.body.classList.add('printing-statement');
        window.print();
        setTimeout(() => document.body.classList.remove('printing-statement'), 500);
    });

    // Toggle for Reports: Milk by Deliverer vs Expenses by Category
    const btnRepMilk = document.getElementById('btn-rep-exp-toggle-milk');
    const btnRepCats = document.getElementById('btn-rep-exp-toggle-cats');
    const repMilkWrapp = document.getElementById('rep-milk-table-wrapper');
    const repCatsWrapp = document.getElementById('rep-cats-table-wrapper');
    if (btnRepMilk && btnRepCats) {
        btnRepMilk.addEventListener('click', () => {
            btnRepMilk.classList.add('active');
            btnRepCats.classList.remove('active');
            if (repMilkWrapp) repMilkWrapp.hidden = false;
            if (repCatsWrapp) repCatsWrapp.hidden = true;
        });
        btnRepCats.addEventListener('click', () => {
            btnRepCats.classList.add('active');
            btnRepMilk.classList.remove('active');
            if (repCatsWrapp) repCatsWrapp.hidden = false;
            if (repMilkWrapp) repMilkWrapp.hidden = true;
        });
    }

    document.getElementById('btn-clear-cart').onclick = async () => {
        if (!cart.length) return;
        if (!await confirmDialog(tr('confirm_clear_cart'), { danger: false, confirmText: tr('confirm_btn') })) return;
        cart = [];
        posShipping = null;
        updateShippingButton();
        renderCart();
    };
    document.getElementById('btn-quick-sale')?.addEventListener('click', () => openQuickSaleModal());
    wireQuickSaleModal();
    document.getElementById('btn-checkout').onclick = () => placeOrder('Completed', { isPaid: true });
    document.getElementById('btn-quote')?.addEventListener('click', () => placeOrder('Quotation'));
    document.getElementById('btn-draft')?.addEventListener('click', () => placeOrder('Draft'));
    document.getElementById('btn-bill')?.addEventListener('click', () => placeOrder('Completed', { isPaid: false, requireCustomer: true }));
    document.getElementById('pos-customer')?.addEventListener('change', () => updatePosCustomerDebt());
    document.getElementById('btn-filter-debt')?.addEventListener('click', () => {
        custDebtOnly = !custDebtOnly;
        const btn = document.getElementById('btn-filter-debt');
        if (btn) btn.classList.toggle('active', custDebtOnly);
        renderCustomers();
    });
    document.getElementById('btn-pos-print')?.addEventListener('click', () => printPosReceipt());
    document.getElementById('btn-pos-do-print')?.addEventListener('click', () => doPrintPosReceipt());
    document.querySelectorAll('input[name="ppd-layout"]').forEach(r => {
        r.addEventListener('change', () => applyPosPrintLayoutPreview());
    });
    document.querySelectorAll('input[name="ppd-pages"]').forEach(r => {
        r.addEventListener('change', () => {
            const custom = document.querySelector('input[name="ppd-pages"]:checked')?.value === 'custom';
            const range = document.getElementById('ppd-page-range');
            if (range) {
                range.disabled = !custom;
                if (custom) range.focus();
            }
        });
    });
    document.getElementById('btn-pos-add-customer')?.addEventListener('click', () => openCustomerModal(null));
    document.getElementById('btn-pos-return')?.addEventListener('click', () => openPosReturnModal());
    document.getElementById('btn-pos-return-load-sales')?.addEventListener('click', () => loadPosReturnSales());
    document.getElementById('btn-pos-return-lookup')?.addEventListener('click', () => {
        const id = Number(document.getElementById('pos-return-order-id')?.value || 0);
        if (!id) return toast(tr('select_customer_or_order'), 'error');
        openPosReturnOrder(id);
    });
    document.getElementById('pos-return-order-id')?.addEventListener('keydown', e => {
        if (e.key !== 'Enter') return;
        e.preventDefault();
        document.getElementById('btn-pos-return-lookup')?.click();
    });
    document.getElementById('btn-pos-return-back-customer')?.addEventListener('click', () => setPosReturnStep('customer'));
    document.getElementById('btn-pos-return-back-sales')?.addEventListener('click', () => {
        const cid = Number(document.getElementById('pos-return-customer')?.value || 0);
        if (cid) loadPosReturnSales();
        else setPosReturnStep('customer');
    });
    document.getElementById('btn-pos-return-confirm')?.addEventListener('click', () => submitPosReturn());
    document.getElementById('btn-pos-return-quick')?.addEventListener('click', () => {
        closeModal('pos-return-modal');
        openBlindReturnModal();
    });
    document.getElementById('btn-confirm-blind-return')?.addEventListener('click', () => submitBlindReturn());
    document.getElementById('blind-search')?.addEventListener('keydown', e => {
        if (e.key !== 'Enter') return;
        e.preventDefault();
        const code = e.target.value.trim();
        const p = findProductByScan(code);
        if (!p) return toast(tr('item_not_found'), 'error');
        addBlindReturnProduct(p);
        e.target.value = '';
    });
    document.getElementById('btn-manage-drafts')?.addEventListener('click', () => openDraftsModal());
    document.getElementById('btn-add-shipping')?.addEventListener('click', () => openShippingModal());
    document.getElementById('btn-ship-add-customer')?.addEventListener('click', () => openCustomerModal(null));
    document.getElementById('btn-clear-shipping')?.addEventListener('click', () => {
        posShipping = null;
        document.getElementById('shipping-form')?.reset();
        updateShippingButton();
        closeModal('shipping-modal');
        toast(tr('saved_ok'), 'success');
    });
    document.getElementById('shipping-form')?.addEventListener('submit', e => {
        e.preventDefault();
        const cust = document.getElementById('ship-customer');
        posShipping = {
            customerId: cust?.value ? Number(cust.value) : null,
            shippingTo: document.getElementById('ship-to').value.trim(),
            orderDate: document.getElementById('ship-order-date').value || null,
            deliveryDate: document.getElementById('ship-delivery-date').value || null,
            dueDate: document.getElementById('ship-due-date').value || null
        };
        if (!posShipping.shippingTo) return toast(tr('shipping_to'), 'error');
        if (posShipping.customerId) {
            const sel = document.getElementById('pos-customer');
            if (sel) sel.value = String(posShipping.customerId);
        }
        const shipChk = document.getElementById('pos-ship');
        if (shipChk && !shipChk.checked) { shipChk.checked = true; updatePosTotalsUi(); }
        updateShippingButton();
        closeModal('shipping-modal');
        toast(tr('shipping_saved'), 'success');
    });
    ['pos-vat', 'pos-ship', 'pos-disc'].forEach(id => {
        document.getElementById(id)?.addEventListener('change', () => {
            clearPosTotalManual();
            updatePosTotalsUi();
        });
    });
    ['pos-ship-amt', 'pos-disc-amt'].forEach(id => {
        document.getElementById(id)?.addEventListener('input', () => {
            clearPosTotalManual();
            updatePosTotalsUi();
        });
    });
    const totalAmt = document.getElementById('pos-total-amt');
    totalAmt?.addEventListener('input', () => {
        const display = parsePosDisplayAmount(totalAmt.value);
        posTotalManual = Number.isFinite(display) ? Math.max(0, posDisplayToBase(display)) : 0;
    });
    totalAmt?.addEventListener('blur', () => {
        const display = parsePosDisplayAmount(totalAmt.value);
        posTotalManual = Number.isFinite(display) ? Math.max(0, posDisplayToBase(display)) : 0;
        totalAmt.value = formatPosAmount(posTotalManual);
    });
    document.getElementById('pos-currency')?.addEventListener('change', () => {
        // Keep USD-base manual total; refresh display in the new currency
        renderCart();
    });

    document.getElementById('btn-confirm-return').onclick = async () => {
        const items = [];
        let count = 0;
        let refund = 0;
        document.querySelectorAll('#return-items .ret-qty').forEach(inp => {
            const i = Number(inp.dataset.i), qty = Number(inp.value);
            if (qty > 0 && returnItemsCache[i]) {
                const it = returnItemsCache[i];
                const amt = (Number(it.price) || 0) * qty;
                items.push({ partId: it.partId, qty, refundAmount: amt });
                count += qty;
                refund += amt;
            }
        });
        if (!items.length) return toast(tr('empty_cart_hint'), 'error');
        try {
            await api('/api/return-item', { method: 'POST', body: JSON.stringify({
                orderId: returnOrderId, reason: document.getElementById('return-reason').value.trim() || 'Web return', items
            })});
            closeModal('return-modal');
            toast(tr('return_ok_detail').replace('{0}', String(count)).replace('{1}', money(refund)), 'success');
            await loadData();
        } catch (err) { toast(err.message, 'error'); }
    };

    document.getElementById('report-preset')?.addEventListener('change', async () => {
        const preset = document.getElementById('report-preset')?.value || 'Monthly';
        syncReportDateInputs(preset);
        await loadReports();
        renderReports();
    });
    const onReportDates = async () => {
        const preset = document.getElementById('report-preset');
        if (preset && preset.value !== 'Custom') preset.value = 'Custom';
        await loadReports();
        renderReports();
    };
    document.getElementById('report-from')?.addEventListener('change', onReportDates);
    document.getElementById('report-to')?.addEventListener('change', onReportDates);
    document.getElementById('history-kind').onchange = async () => { await loadHistory(); renderHistory(); };
    document.getElementById('btn-print-barcodes').onclick = () => openBarcodePrintPreview();
    document.getElementById('btn-barcode-do-print')?.addEventListener('click', () => printBarcodePreview());
    document.querySelectorAll('input[name="bpd-layout"]').forEach(r => {
        r.addEventListener('change', () => applyBarcodePrintLayoutPreview());
    });
    document.querySelectorAll('input[name="bpd-pages"]').forEach(r => {
        r.addEventListener('change', () => {
            const custom = document.querySelector('input[name="bpd-pages"]:checked')?.value === 'custom';
            const range = document.getElementById('bpd-page-range');
            if (range) {
                range.disabled = !custom;
                if (custom) range.focus();
            }
        });
    });
    document.getElementById('bar-select-all')?.addEventListener('change', e => {
        const keys = filteredBarcodeItems().slice(0, 120).map(barcodeItemKey);
        if (e.target.checked) keys.forEach(k => barcodeSelected.add(k));
        else keys.forEach(k => barcodeSelected.delete(k));
        renderBarcodes();
    });

    document.querySelectorAll('input[name="p-sell-by"]').forEach(r => {
        r.addEventListener('change', () => onProductTypeChange());
    });
    document.querySelectorAll('input[name="p-type"]').forEach(r => {
        r.addEventListener('change', () => onProductTypeChange());
    });

    document.querySelectorAll('[data-close]').forEach(btn => btn.onclick = () => closeModal(btn.getAttribute('data-close')));
    document.querySelectorAll('.modal-overlay:not(#confirm-modal)').forEach(ov => ov.addEventListener('click', e => {
        if (e.target === ov) {
            if (ov.id === 'license-modal' && document.getElementById('license-modal')?.classList.contains('license-wall-blocking')) {
                return;
            }
            closeModal(ov.id);
        }
    }));

    document.getElementById('inv-search').oninput = () => renderInventory();
    document.getElementById('inv-filter')?.addEventListener('change', e => { invFilter = e.target.value; renderInventory(); });
    document.getElementById('btn-inv-table')?.addEventListener('click', () => {
        invView = 'table';
        renderInventory();
    });
    document.getElementById('btn-inv-cards')?.addEventListener('click', () => {
        invView = 'card';
        renderInventory();
    });
    document.getElementById('inv-select-all')?.addEventListener('change', e => {
        const list = filteredProducts(document.getElementById('inv-search')?.value, invCat, { invMode: true });
        if (e.target.checked) list.forEach(p => invSelected.add(p.id));
        else list.forEach(p => invSelected.delete(p.id));
        renderInventory();
    });
    document.getElementById('btn-bulk-delete-inventory')?.addEventListener('click', async () => {
        if (!invSelected.size || !await confirmDialog(tr('confirm_bulk_delete'))) return;
        try {
            await api('/api/products/bulk-delete', { method: 'POST', body: JSON.stringify({ ids: [...invSelected] }) });
            invSelected.clear();
            toast(tr('deleted_ok'), 'success');
            await loadData();
        } catch (err) { toast(err.message, 'error'); }
    });
    document.getElementById('pos-search')?.addEventListener('input', () => {
        clearTimeout(posSearchDebounce);
        posSearchDebounce = setTimeout(() => renderPOS(), 120);
    });
    // Capture barcode wedge keys anywhere on POS (search field or page)
    document.addEventListener('keydown', handlePosScannerKeydown, true);
    // Keep search focused when opening POS for easier scanning
    document.querySelector('.nav-item[data-target="pos"]')?.addEventListener('click', () => {
        setTimeout(() => document.getElementById('pos-search')?.focus(), 80);
    });
    document.getElementById('pos-cat-prev')?.addEventListener('click', () => scrollPosCats(-1));
    document.getElementById('pos-cat-next')?.addEventListener('click', () => scrollPosCats(1));
    document.getElementById('cust-search').oninput = () => renderCustomers();
    document.getElementById('supp-search').oninput = () => renderSuppliers();
    document.getElementById('bar-search').oninput = () => renderBarcodes();

    const refreshWithToast = async (fn) => {
        try {
            if (typeof fn === 'function') await fn();
            else await loadData();
            toast(tr('app_refreshed'), 'success');
        } catch (e) { console.error(e); }
    };

    document.getElementById('btn-refresh-dashboard')?.addEventListener('click', () => refreshWithToast());
    document.getElementById('btn-refresh-inventory')?.addEventListener('click', () => refreshWithToast());
    document.getElementById('btn-refresh-sales')?.addEventListener('click', () => refreshWithToast());
    document.getElementById('btn-refresh-reports')?.addEventListener('click', () => refreshWithToast(async () => { await loadReports(); renderReports(); }));
    document.getElementById('btn-refresh-history')?.addEventListener('click', () => refreshWithToast(async () => { await loadHistory(); renderHistory(); }));
    document.getElementById('btn-refresh-quotations')?.addEventListener('click', () => refreshWithToast());
    document.getElementById('btn-quote-print')?.addEventListener('click', () => printQuotationPreview());
    document.getElementById('btn-quote-export')?.addEventListener('click', () => exportQuotationPreview());
    document.getElementById('btn-refresh-customers')?.addEventListener('click', () => refreshWithToast());
    document.getElementById('btn-refresh-suppliers')?.addEventListener('click', () => refreshWithToast());
    document.getElementById('btn-refresh-expenses')?.addEventListener('click', () => refreshWithToast());
    document.getElementById('btn-refresh-barcodes')?.addEventListener('click', () => refreshWithToast());
    document.getElementById('btn-refresh-users')?.addEventListener('click', () => refreshWithToast());

    const btnExpInv = document.getElementById('btn-export-inventory');
    if (btnExpInv) btnExpInv.onclick = () => downloadServerExport(
        '/api/products/export?format=xlsx', 'inventory.xlsx',
        () => exportCsv(products.map(productExportRow), 'inventory.csv'));
    const btnExpCust = document.getElementById('btn-export-customers');
    if (btnExpCust) btnExpCust.onclick = () => downloadServerExport(
        '/api/customers/export?format=xlsx', 'customers.xlsx',
        () => exportCsv(customers.map(customerExportRow), 'customers.csv'));
    const btnExpSupp = document.getElementById('btn-export-suppliers');
    if (btnExpSupp) btnExpSupp.onclick = () => downloadServerExport(
        '/api/suppliers/export?format=xlsx', 'suppliers.xlsx',
        () => exportCsv(suppliers.map(supplierExportRow), 'suppliers.csv'));
    document.getElementById('btn-export-sales')?.addEventListener('click', () => downloadServerExport(
        '/api/sales/export?format=xlsx', 'sales.xlsx',
        () => exportCsv(sales.map(o => ({
            orderId: o.orderId, date: o.date, customer: o.customer, total: o.total,
            payment: o.paymentStatus || o.payment || ''
        })), 'sales.csv')));
    document.getElementById('btn-export-reports')?.addEventListener('click', () => {
        const s = reportSummary || {};
        const from = document.getElementById('report-from')?.value || toInputDate(s.fromDate) || '';
        const to = document.getElementById('report-to')?.value || toInputDate(s.toDate) || '';
        const qs = new URLSearchParams({ format: 'xlsx' });
        if (from) qs.set('from', from);
        if (to) qs.set('to', to);
        downloadServerExport('/api/reports/export?' + qs.toString(), 'report.xlsx', () => {
            const rows = [
                { row_type: 'summary', metric: tr('date_from'), value: toInputDate(s.fromDate) || '' },
                { row_type: 'summary', metric: tr('date_to'), value: toInputDate(s.toDate) || '' },
                { row_type: 'summary', metric: tr('rep_sales'), value: s.totalSales ?? 0 },
                { row_type: 'summary', metric: tr('rep_cost'), value: s.totalCost ?? 0 },
                { row_type: 'summary', metric: tr('rep_expenses'), value: s.totalExpenses ?? 0 },
                { row_type: 'summary', metric: tr('rep_profit_before_expenses'), value: s.totalProfit ?? 0 },
                { row_type: 'summary', metric: tr('rep_profit_after_expenses'), value: s.totalProfitAfterExpenses ?? 0 },
                ...(reportTop || []).map(r => ({
                    row_type: 'product',
                    name: r.product_name || r.ProductName || '',
                    qty: r.quantity_sold ?? r.QuantitySold ?? 0,
                    sales: r.total_sales ?? r.TotalSales ?? 0,
                    profit: r.profit ?? r.Profit ?? 0
                }))
            ];
            exportCsv(rows, 'report-export.csv');
        });
    });
    document.getElementById('btn-export-history')?.addEventListener('click', () => downloadServerExport(
        '/api/history/export?format=xlsx', 'history.xlsx',
        () => exportCsv((window._historyRows || []).map(historyExportRow), 'history.csv')));
    document.getElementById('btn-export-expenses')?.addEventListener('click', () =>
        downloadServerExport('/api/expenses/export?format=xlsx', 'expenses.xlsx', () =>
            exportCsv(expenses.map(e => ({
                date: e.expenseDate, category: e.category, deliverer: e.supplierName || '',
                liters: e.liters || '', unit_price: e.unitPrice || '', milk_type: e.milkType || '', shift: e.shift || '',
                amount: e.amount, description: e.description,
                paid: e.isPaid ? 1 : 0, recurring: e.isRecurring ? 1 : 0
            })), 'expenses.csv')));

    document.getElementById('btn-activate-license')?.addEventListener('click', openLicenseModal);
    document.getElementById('btn-copy-hwid')?.addEventListener('click', async () => {
        const text = document.getElementById('lic-hwid')?.textContent || '';
        try {
            await navigator.clipboard.writeText(text);
            toast(tr('copied'), 'success');
        } catch {
            toast(text, 'success');
        }
    });
    document.getElementById('btn-confirm-activate')?.addEventListener('click', async () => {
        const err = document.getElementById('license-activate-error');
        err?.classList.remove('visible');
        const licenseKey = document.getElementById('lic-key')?.value.trim() || '';
        if (!licenseKey.replace(/[-\s]/g, '').length) {
            if (err) { err.textContent = tr('license_invalid'); err.classList.add('visible'); }
            return;
        }
        try {
            await api('/api/license/activate', {
                method: 'POST',
                body: JSON.stringify({ licenseKey, customerName: 'Licensed User' })
            });
            window._license = await api('/api/license');
            document.getElementById('license-modal')?.classList.remove('license-wall-blocking');
            checkLicenseExpirationWall();
            closeModal('license-modal');
            toast(tr('license_ok'), 'success');
            renderLicense();
            applyI18n();
        } catch (e) {
            if (err) { err.textContent = e.message || tr('license_invalid'); err.classList.add('visible'); }
        }
    });
    document.getElementById('btn-start-trial')?.addEventListener('click', async () => {
        const err = document.getElementById('license-activate-error');
        err?.classList.remove('visible');
        try {
            await api('/api/license/start-trial', { method: 'POST', body: '{}' });
            closeModal('license-modal');
            toast(tr('trial_ok'), 'success');
            await loadLicense();
            renderLicense();
        } catch (e) {
            if (err) { err.textContent = e.message || tr('trial_used'); err.classList.add('visible'); }
        }
    });
}

function postHost(action) {
    try {
        if (window.chrome?.webview?.postMessage)
            window.chrome.webview.postMessage(JSON.stringify({ action }));
    } catch { }
}

function setupChrome() {
    const isHosted = !!(window.chrome?.webview);
    document.body.classList.toggle('desktop-host', isHosted);
    document.body.classList.toggle('web-client', !isHosted);

    document.getElementById('win-min')?.addEventListener('click', () => postHost('minimize'));
    document.getElementById('win-max')?.addEventListener('click', () => postHost('maximize'));
    document.getElementById('win-close')?.addEventListener('click', () => postHost('close'));
    // Drag from the title bar (empty areas + drag region). Must post while
    // the left button is still down so the host can start the native move loop.
    const titlebar = document.getElementById('titlebar');
    const startDrag = (e) => {
        if (!isHosted) return;
        if (typeof e.button === 'number' && e.button !== 0) return;
        if (e.target.closest('button, a, input, select, textarea, label, .tb-btn, .tb-user, .win-btn, .window-controls')) return;
        e.preventDefault();
        e.stopPropagation();
        postHost('drag');
    };
    if (titlebar) {
        titlebar.addEventListener('pointerdown', startDrag, true);
        titlebar.addEventListener('dblclick', e => {
            if (e.target.closest('button, a, input, select, textarea, label, .tb-btn, .tb-user, .win-btn, .window-controls')) return;
            postHost('maximize');
        });
    }
    try {
        if (window.chrome?.webview) {
            window.chrome.webview.addEventListener('message', ev => {
                let data = ev.data;
                if (typeof data === 'string') {
                    try { data = JSON.parse(data); } catch { return; }
                }
                if (data?.type === 'windowState') {
                    const icon = document.getElementById('win-max-icon');
                    if (icon) icon.textContent = data.maximized ? 'filter_none' : 'crop_square';
                }
                if (data?.type === 'printers') {
                    fillBarcodePrinterSelect(data.printers || [], data.defaultPrinter || '');
                }
                if (data?.type === 'printResult') {
                    if (data.ok) {
                        toast(tr('print_sent'));
                        try { loadPrintSettings(); } catch { /* ignore */ }
                    } else {
                        toast(data.message || tr('print_failed'), 'error');
                    }
                }
            });
        }
    } catch { }
}

let calcExpr = '0';
function updateCalcDisplay() {
    const el = document.getElementById('calc-display');
    if (el) el.textContent = calcExpr;
}
function calcPress(key) {
    if (key === 'C') { calcExpr = '0'; updateCalcDisplay(); return; }
    if (key === '±') {
        if (calcExpr.startsWith('-')) calcExpr = calcExpr.slice(1);
        else if (calcExpr !== '0') calcExpr = '-' + calcExpr;
        updateCalcDisplay(); return;
    }
    if (key === '%') {
        try { calcExpr = String(Function('"use strict";return (' + calcExpr + ')/100')()); } catch { calcExpr = '0'; }
        updateCalcDisplay(); return;
    }
    if (key === '=') {
        try {
            const safe = calcExpr.replace(/[^0-9+\-*/().%\s]/g, '');
            calcExpr = String(Function('"use strict";return (' + safe + ')')());
        } catch { calcExpr = 'Error'; }
        updateCalcDisplay(); return;
    }
    if (calcExpr === '0' || calcExpr === 'Error') calcExpr = /[0-9.]/.test(key) ? key : '0' + key;
    else calcExpr += key;
    updateCalcDisplay();
}

const NOTIF_DISMISS_KEY = 'generic_dismissed_notifs';

function notifKey(n) {
    if (!n) return '';
    if (n.key) return n.key;
    const type = n.type || '';
    const target = n.target || '';
    const raw = n.message || n.title || '';
    const clean = raw.replace(/[\d]+|\(.*?\)|(low|out of stock|alert|منخفض|متبقي|نافد|تنبيه|نقص|المخزون)/gi, '').trim();
    return `${type}|${target}|${clean}`;
}

const NOTIF_TARGETS = {
    btnInventory: 'inventory',
    btnCustomers: 'customers',
    btnSuppliers: 'suppliers',
    btnHistory: 'history',
    btnMonthlyExpenses: 'expenses'
};

function notificationsUrl() {
    return '/api/notifications?lang=' + encodeURIComponent(lang === 'ar' ? 'ar' : 'en');
}

function setNotifBadge(count) {
    const n = Number(count) || 0;
    ['notif-badge', 'notif-badge-mobile'].forEach(id => {
        const badge = document.getElementById(id);
        if (!badge) return;
        if (n > 0) {
            badge.hidden = false;
            badge.textContent = n > 99 ? '99+' : String(n);
        } else {
            badge.hidden = true;
            badge.textContent = '';
        }
    });
}

function loadDismissedNotifs() {
    try {
        const raw = localStorage.getItem(NOTIF_DISMISS_KEY);
        const arr = raw ? JSON.parse(raw) : [];
        return new Set(Array.isArray(arr) ? arr : []);
    } catch { return new Set(); }
}

function saveDismissedNotifs(set) {
    localStorage.setItem(NOTIF_DISMISS_KEY, JSON.stringify([...set]));
}

function filterActiveNotifications(list) {
    const dismissed = loadDismissedNotifs();
    return (Array.isArray(list) ? list : []).filter(n => !dismissed.has(notifKey(n)));
}

function dismissNotification(key) {
    const dismissed = loadDismissedNotifs();
    dismissed.add(key);
    saveDismissedNotifs(dismissed);
}

function dismissAllNotifications(list) {
    const dismissed = loadDismissedNotifs();
    (list || []).forEach(n => dismissed.add(notifKey(n)));
    saveDismissedNotifs(dismissed);
}

function openNotifTarget(target) {
    const page = NOTIF_TARGETS[target] || target;
    if (!page || !document.getElementById(page)) return;
    closeModal('notif-modal');
    navigateTo(page, true);
}

function renderNotificationList(list) {
    const box = document.getElementById('notif-list');
    const clearAllBtn = document.getElementById('btn-notif-clear-all');
    if (!box) return;
    const active = filterActiveNotifications(list);
    if (clearAllBtn) clearAllBtn.hidden = !active.length;
    if (!active.length) {
        box.innerHTML = `<div class="empty-state">${tr('no_notifications')}</div>`;
        return;
    }
    box.innerHTML = active.map(n => {
        const key = notifKey(n);
        const enc = encodeURIComponent(key);
        const target = encodeURIComponent(n.target || '');
        return `<div class="notif-item" data-notif-target="${target}">
            <div class="notif-item-body">
                <strong>${escapeHtml(n.title || n.type || '')}</strong>
                <p>${escapeHtml(n.message || '')}</p>
            </div>
            <button type="button" class="btn-icon notif-clear-one" data-dismiss-notif="${enc}" title="${escapeHtml(tr('clear_notification'))}">
                <span class="material-symbols-rounded">delete</span>
            </button>
        </div>`;
    }).join('');
    box.querySelectorAll('[data-dismiss-notif]').forEach(btn => {
        btn.onclick = (e) => {
            e.preventDefault();
            e.stopPropagation();
            dismissNotification(decodeURIComponent(btn.dataset.dismissNotif));
            renderNotificationList(list);
            refreshNotifications();
        };
    });
    box.querySelectorAll('.notif-item').forEach(item => {
        item.onclick = () => {
            const target = decodeURIComponent(item.getAttribute('data-notif-target') || '');
            if (target) openNotifTarget(target);
        };
    });
}

async function refreshNotifications() {
    try {
        const list = await api(notificationsUrl());
        const active = filterActiveNotifications(list);
        setNotifBadge(active.length);
        return Array.isArray(list) ? list : [];
    } catch {
        setNotifBadge(0);
        return [];
    }
}

async function openNotifications() {
    const list = await refreshNotifications();
    renderNotificationList(list);
    openModal('notif-modal');
}

async function openBackupModal() {
    try {
        const st = await api('/api/backup/status');
        const status = document.getElementById('backup-status');
        if (status) {
            let text = st.lastBackup
                ? `${tr('last_backup')}: ${formatDate(st.lastBackup)}`
                : tr('backup_none');
            if (st.autoSchedule && st.autoSchedule !== 'off') {
                const schedKey = 'auto_backup_' + st.autoSchedule;
                text += ` · ${tr('auto_backup')}: ${tr(schedKey)}`;
                if (st.lastAutoBackup) text += ` (${formatDate(st.lastAutoBackup)})`;
            }
            status.textContent = text;
        }
        const sel = document.getElementById('backup-auto-schedule');
        if (sel) sel.value = st.autoSchedule || 'off';
        const hint = document.getElementById('backup-auto-hint');
        if (hint) {
            hint.textContent = (st.autoSchedule && st.autoSchedule !== 'off')
                ? tr('auto_backup_hint_on')
                : tr('auto_backup_hint_off');
        }
        const pathEl = document.getElementById('backup-folder-path');
        if (pathEl) pathEl.textContent = st.folder || '—';
        const resetFolder = document.getElementById('btn-backup-reset-folder');
        if (resetFolder) resetFolder.hidden = !st.customFolder;
        const files = document.getElementById('backup-files');
        if (files) {
            files.innerHTML = (st.files || []).map(f =>
                `<div class="backup-file-row">
                    <span>${escapeHtml(f.name)} · ${formatDate(f.modified)}</span>
                    <button type="button" class="btn btn-secondary btn-sm" data-restore="${escapeHtml(f.name)}">${tr('restore_backup')}</button>
                </div>`).join('') || '';
            files.querySelectorAll('[data-restore]').forEach(btn => {
                btn.onclick = async () => {
                    const fileName = btn.getAttribute('data-restore');
                    if (!await confirmDialog(tr('confirm_restore'), { danger: false, confirmText: tr('restore_backup') })) return;
                    try {
                        await api('/api/backup/restore', { method: 'POST', body: JSON.stringify({ fileName }) });
                        toast(tr('saved_ok'), 'success');
                        closeModal('backup-modal');
                        await loadData();
                    } catch (e) { toast(e.message, 'error'); }
                };
            });
        }
    } catch (e) {
        document.getElementById('backup-status').textContent = e.message;
    }
    openModal('backup-modal');
}

function showLockOverlay() {
    const ov = document.getElementById('lock-overlay');
    if (!ov) return;
    ov.hidden = false;
    const pass = document.getElementById('lock-pass');
    if (pass) { pass.value = ''; pass.focus(); }
}

function setupTools() {
    document.getElementById('btn-notif')?.addEventListener('click', openNotifications);
    document.getElementById('btn-notif-mobile')?.addEventListener('click', openNotifications);
    document.getElementById('btn-notif-clear-all')?.addEventListener('click', async () => {
        const list = await refreshNotifications();
        const active = filterActiveNotifications(list);
        if (!active.length) return;
        if (!await confirmDialog(tr('confirm_clear_notifications'), { danger: true, confirmText: tr('clear_all_notifications') })) return;
        dismissAllNotifications(active);
        renderNotificationList(list);
        refreshNotifications();
    });
    document.getElementById('btn-calc')?.addEventListener('click', () => {
        calcExpr = '0'; updateCalcDisplay(); openModal('calc-modal');
    });
    document.getElementById('btn-calc-mobile')?.addEventListener('click', () => {
        calcExpr = '0'; updateCalcDisplay(); openModal('calc-modal');
    });
    document.getElementById('btn-backup')?.addEventListener('click', openBackupModal);
    document.getElementById('btn-backup-mobile')?.addEventListener('click', openBackupModal);
    document.getElementById('backup-auto-schedule')?.addEventListener('change', async e => {
        try {
            await api('/api/backup/auto', {
                method: 'PUT',
                body: JSON.stringify({ schedule: e.target.value })
            });
            toast(tr('saved_ok'), 'success');
            await openBackupModal();
        } catch (err) { toast(err.message || tr('backup_fail'), 'error'); }
    });
    document.getElementById('btn-backup-choose-folder')?.addEventListener('click', async () => {
        try {
            const res = await api('/api/backup/choose-folder', { method: 'POST', body: '{}' });
            if (res?.cancelled) return;
            toast(tr('saved_ok'), 'success');
            await openBackupModal();
        } catch (err) { toast(err.message || tr('backup_fail'), 'error'); }
    });
    document.getElementById('btn-backup-reset-folder')?.addEventListener('click', async () => {
        try {
            await api('/api/backup/reset-folder', { method: 'POST', body: '{}' });
            toast(tr('saved_ok'), 'success');
            await openBackupModal();
        } catch (err) { toast(err.message || tr('backup_fail'), 'error'); }
    });
    document.getElementById('btn-about')?.addEventListener('click', () => openModal('about-modal'));
    document.getElementById('btn-about-mobile')?.addEventListener('click', () => openModal('about-modal'));
    document.getElementById('btn-contact-support')?.addEventListener('click', () => {
        const mail = 'mailto:softioservices@gmail.com';
        try {
            if (window.chrome?.webview?.postMessage)
                window.chrome.webview.postMessage(JSON.stringify({ action: 'openUrl', url: mail }));
            else
                window.open(mail, '_blank');
        } catch {
            toast('softioservices@gmail.com', 'success');
        }
    });
    document.getElementById('btn-lock')?.addEventListener('click', showLockOverlay);
    document.getElementById('btn-lock-mobile')?.addEventListener('click', showLockOverlay);
    document.getElementById('lock-form')?.addEventListener('submit', async e => {
        e.preventDefault();
        const err = document.getElementById('lock-error');
        err?.classList.remove('visible');
        try {
            await api('/api/verify-password', {
                method: 'POST',
                body: JSON.stringify({
                    username: currentUser?.username || '',
                    password: document.getElementById('lock-pass').value
                })
            });
            document.getElementById('lock-overlay').hidden = true;
        } catch {
            if (err) { err.textContent = tr('unlock_fail'); err.classList.add('visible'); }
        }
    });
    document.querySelectorAll('[data-calc]').forEach(btn => {
        btn.addEventListener('click', () => calcPress(btn.getAttribute('data-calc')));
    });
    document.getElementById('btn-backup-export')?.addEventListener('click', async () => {
        try {
            const res = await fetch(API + '/api/backup/export');
            if (!res.ok) throw new Error((await res.json().catch(() => ({}))).error || res.statusText);
            const blob = await res.blob();
            const cd = res.headers.get('Content-Disposition') || '';
            const m = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/i.exec(cd);
            const name = m ? m[1].replace(/['"]/g, '') : `backup_${Date.now()}.db`;
            const a = document.createElement('a');
            a.href = URL.createObjectURL(blob);
            a.download = name;
            a.click();
            URL.revokeObjectURL(a.href);
            toast(tr('backup_export_ok'), 'success');
            openBackupModal();
        } catch (e) { toast(e.message || tr('backup_fail'), 'error'); }
    });
    document.getElementById('btn-backup-import')?.addEventListener('click', () => {
        document.getElementById('backup-import-file')?.click();
    });
    document.getElementById('backup-import-file')?.addEventListener('change', async (e) => {
        const file = e.target.files?.[0];
        e.target.value = '';
        if (!file) return;
        if (!await confirmDialog(tr('confirm_restore'), { danger: true, confirmText: tr('import') })) return;
        try {
            const fd = new FormData();
            fd.append('file', file);
            const res = await fetch(API + '/api/backup/import', { method: 'POST', body: fd });
            if (!res.ok) {
                let err = res.statusText;
                try { const j = await res.json(); err = j.error || j.title || err; } catch {}
                throw new Error(err);
            }
            toast(tr('backup_import_ok'), 'success');
            closeModal('backup-modal');
            await loadData();
        } catch (err) { toast(err.message, 'error'); }
    });
    document.getElementById('btn-backup-factory')?.addEventListener('click', async () => {
        if (!can('admin')) return;
        if (!await confirmDialog(tr('confirm_factory_1'), { danger: true, confirmText: tr('factory_reset') })) return;
        if (!await confirmDialog(tr('confirm_factory_2'), { danger: true, confirmText: tr('factory_reset') })) return;
        const password = await promptDialog({
            title: tr('factory_reset'),
            message: tr('locked_subtitle'),
            confirmText: tr('factory_reset'),
            cancelText: tr('cancel'),
            inputType: 'password',
            autocomplete: 'current-password',
            maxLength: 128
        });
        if (password == null || password === '') return;
        try {
            await api('/api/backup/factory-reset', {
                method: 'POST',
                body: JSON.stringify({ username: currentUser?.username, password })
            });
            toast(tr('factory_ok'), 'success');
            closeModal('backup-modal');
            await loadData();
        } catch (e) { toast(e.message || tr('factory_fail'), 'error'); }
    });
    document.getElementById('btn-open-backup')?.addEventListener('click', async () => {
        try { await api('/api/backup/open-folder', { method: 'POST', body: '{}' }); }
        catch (e) { toast(e.message, 'error'); }
    });
}

function setupSignalR() {
    if (!window.signalR) return;
    const connection = new signalR.HubConnectionBuilder().withUrl('/hubs/inventory').withAutomaticReconnect().build();
    const onInventoryEvent = () => { loadData({ fromSignalR: true }); };
    connection.on('InventoryChanged', onInventoryEvent);
    connection.on('SaleCompleted', onInventoryEvent);
    connection.on('StockUpdated', onInventoryEvent);
    connection.on('ScaleWeight', () => { /* scale branch only */ });
    connection.on('ScaleStatus', () => { /* scale branch only */ });
    connection.start().catch(() => setTimeout(setupSignalR, 5000));
}

const scaleManager = {
    pending: null, // { weightKg, linePrice, productId } after Read / manual entry
    manualMode: false,

    async init() {
        const toggleSettings = (e) => {
            if (e) { e.preventDefault(); e.stopPropagation(); }
            const settings = document.getElementById('scaleSettings');
            const panel = document.getElementById('scalePanel');
            if (!settings || !panel) return;
            panel.hidden = false;
            const opening = settings.classList.contains('hidden');
            settings.classList.toggle('hidden', !opening);
            panel.classList.toggle('settings-open', opening);
            if (opening) {
                this.refreshPorts();
                this.refreshStatus(true);
            }
        };
        const gearBtn = document.getElementById('btnScaleSettings');
        if (gearBtn) gearBtn.onclick = toggleSettings;
        document.getElementById('btnScaleConnect')?.addEventListener('click', () => this.connect());
        document.getElementById('btnScaleDisconnect')?.addEventListener('click', () => this.disconnect());
        document.getElementById('btnScaleTare')?.addEventListener('click', () => this.tare());
        document.getElementById('btnScaleZero')?.addEventListener('click', () => this.zero());
        document.getElementById('btnScaleRequest')?.addEventListener('click', () => this.requestWeight());
        document.getElementById('btnScaleSimulate')?.addEventListener('click', () => this.simulate(0.525));
        document.getElementById('btnScaleAddWeighed')?.addEventListener('click', () => this.addWeighedProduct());
        document.getElementById('btnScaleClearProduct')?.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            this.clearSelectedProduct();
        });
        const manual = document.getElementById('scaleManualWeight');
        if (manual) {
            manual.addEventListener('input', () => this.onManualWeightInput());
            manual.addEventListener('change', () => this.onManualWeightInput());
            manual.addEventListener('keydown', (e) => {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    this.addWeighedProduct();
                }
            });
        }
        await this.refreshPorts();
        await this.refreshStatus();
        this.renderSelected();
        this.updateManualHint();
        setInterval(() => this.refreshStatus(true), 4000);
    },

    getSelectedProduct() {
        if (!lastTappedProductId) return null;
        const p = products.find(x => x.id === lastTappedProductId);
        return p && isSellByWeight(p) ? p : null;
    },

    clearSelectedProduct() {
        lastTappedProductId = null;
        this.pending = null;
        this.manualMode = false;
        this.setManualWeight(0);
        const nameEl = document.getElementById('scaleSelectedName');
        const rateEl = document.getElementById('scaleSelectedRate');
        if (nameEl) nameEl.textContent = '';
        if (rateEl) rateEl.textContent = '';
        this.renderSelected();
        this.renderCalc(0, false);
        this.updateManualHint();
        // Refresh cards so weigh highlight is cleared (don't re-select)
        const grid = document.getElementById('pos-products');
        if (grid) {
            grid.querySelectorAll('.pos-product-card.is-scale-selected').forEach(el => {
                el.classList.remove('is-scale-selected');
            });
        }
    },

    selectForWeighing(p) {
        if (!p) return;
        lastTappedProductId = p.id;
        const panel = document.getElementById('scalePanel');
        if (panel) {
            panel.hidden = false;
            panel.classList.remove('is-collapsed');
        }
        this.renderSelected();
        const currentW = this.getManualWeight();
        if (currentW > 0) {
            this.pending = {
                productId: p.id,
                weightKg: currentW,
                linePrice: this.calcLinePrice(p, currentW),
                source: 'manual'
            };
            this.addWeighedProduct();
        } else {
            const input = document.getElementById('scaleManualWeight');
            if (input) {
                input.focus();
                input.select();
            }
            toast(tr('weigh_first') || `${p.name} — ${tr('scale_weight_lbl')}`, 'info');
        }
    },

    setSelectedProduct(p) {
        this.selectForWeighing(p);
    },

    setManualWeight(kg) {
        const el = document.getElementById('scaleManualWeight');
        if (el) el.value = Math.round(Number(kg || 0) * 1000);
    },

    getManualWeight() {
        const el = document.getElementById('scaleManualWeight');
        const g = Number(el?.value);
        return Number.isFinite(g) ? Math.max(0, g / 1000) : 0;
    },

    onManualWeightInput() {
        this.manualMode = true;
        const product = this.getSelectedProduct();
        const weightKg = this.getManualWeight();
        if (!product) {
            this.pending = null;
            this.renderCalc(weightKg, false);
            return;
        }
        if (weightKg <= 0) {
            this.pending = null;
            this.renderCalc(0, false);
            return;
        }
        this.pending = {
            productId: product.id,
            weightKg,
            linePrice: this.calcLinePrice(product, weightKg),
            source: 'manual'
        };
        this.renderCalc(weightKg, true);
    },

    commitPendingFromWeight(weightKg, source = 'scale') {
        const product = this.getSelectedProduct();
        if (!product || weightKg <= 0) {
            this.pending = null;
            return false;
        }
        this.pending = {
            productId: product.id,
            weightKg,
            linePrice: this.calcLinePrice(product, weightKg),
            source
        };
        return true;
    },

    weightKgNow() {
        if (this.manualMode) return this.getManualWeight();
        let weight = Number(scaleState.weight || 0);
        const unit = (scaleState.unit || 'kg').toLowerCase();
        if (unit === 'g') weight = weight / 1000;
        return weight;
    },

    calcLinePrice(product, weightKg) {
        const unitPrice = Number(product?.price) || 0;
        return Math.round(unitPrice * weightKg * 100) / 100;
    },

    updateManualHint() {
        const hint = document.getElementById('scaleManualHint');
        if (!hint) return;
        const g = Number(document.getElementById('scaleManualWeight')?.value || 0);
        if (g > 0) {
            const kg = (g / 1000).toFixed(3);
            hint.textContent = `${g} g = ${kg} ${tr('unit_kg')}`;
        } else {
            hint.textContent = scaleState.connected ? tr('scale_manual_hint') : tr('scale_offline_manual');
        }
    },

    async refreshPorts() {
        try {
            const ports = await api('/api/scale/ports');
            const sel = document.getElementById('scalePortSelect');
            if (!sel) return;
            const current = sel.value || scaleState.port;
            sel.innerHTML = '';
            (ports || []).forEach(p => {
                const opt = document.createElement('option');
                opt.value = p;
                opt.textContent = p;
                sel.appendChild(opt);
            });
            if (current) sel.value = current;
        } catch { }
    },

    async refreshStatus(silent = false) {
        try {
            const data = await api('/api/scale/status');
            this.applyState(data);
            const baud = document.getElementById('scaleBaudSelect');
            const auto = document.getElementById('scaleAutoConnect');
            const port = document.getElementById('scalePortSelect');
            if (baud && data.baudRate) baud.value = String(data.baudRate);
            if (auto) auto.checked = !!data.autoConnect;
            if (port && data.port) {
                if (![...port.options].some(o => o.value === data.port)) {
                    const opt = document.createElement('option');
                    opt.value = data.port;
                    opt.textContent = data.port;
                    port.appendChild(opt);
                }
                port.value = data.port;
            }
        } catch {
            if (!silent) this.applyState({ connected: false, weight: 0, unit: 'kg', stable: true });
        }
    },

    applyState(data) {
        if (!data) return;
        scaleState = {
            connected: !!data.connected,
            weight: Number(data.weight || 0),
            unit: data.unit || 'kg',
            stable: data.stable !== false,
            port: data.port || scaleState.port || ''
        };
        this.updateManualHint();
        // Live scale updates fill the input unless cashier is typing manually
        if (!this.manualMode && scaleState.connected) {
            let w = Number(scaleState.weight || 0);
            if ((scaleState.unit || 'kg').toLowerCase() === 'g') w = w / 1000;
            this.setManualWeight(w);
            const product = this.getSelectedProduct();
            if (product && w > 0) this.commitPendingFromWeight(w, 'scale');
        }
        this.render();
    },

    onWeight(payload) { this.applyState({ ...scaleState, ...payload }); },
    onStatus(payload) { this.applyState({ ...scaleState, ...payload }); },

    renderSelected() {
        const empty = document.getElementById('scaleSelectedEmpty');
        const info = document.getElementById('scaleSelectedInfo');
        const nameEl = document.getElementById('scaleSelectedName');
        const rateEl = document.getElementById('scaleSelectedRate');
        const panel = document.getElementById('scalePanel');
        const p = this.getSelectedProduct();
        if (!p) {
            if (panel) panel.classList.add('is-collapsed');
            if (empty) {
                empty.hidden = false;
                empty.style.display = '';
            }
            if (info) {
                info.hidden = true;
                info.style.display = 'none';
            }
            if (nameEl) nameEl.textContent = '';
            if (rateEl) rateEl.textContent = '';
            return;
        }
        if (panel) panel.classList.remove('is-collapsed');
        if (empty) {
            empty.hidden = true;
            empty.style.display = 'none';
        }
        if (info) {
            info.hidden = false;
            info.style.display = 'flex';
        }
        if (nameEl) nameEl.textContent = p.name;
        if (rateEl) rateEl.textContent = `${tr('scale_selected')}: ${formatPosPrice(p)}`;
    },

    renderCalc(weightKg, locked = false) {
        const calcEl = document.getElementById('scaleCalcPrice');
        const p = this.getSelectedProduct();
        if (!calcEl) return;
        if (!p) {
            calcEl.textContent = money(0);
            calcEl.classList.remove('is-ready');
            return;
        }
        const w = weightKg != null ? weightKg : (this.pending?.weightKg ?? this.getManualWeight());
        const price = this.calcLinePrice(p, Math.max(0, w));
        calcEl.textContent = money(price);
        calcEl.classList.toggle('is-ready', locked || (this.pending && this.pending.productId === p.id && w > 0));
    },

    render() {
        const dot = document.getElementById('scaleDot');
        const text = document.getElementById('scaleStatusText');
        if (dot) {
            dot.classList.toggle('on', scaleState.connected && scaleState.stable);
            dot.classList.toggle('unstable', scaleState.connected && !scaleState.stable);
        }
        if (text) {
            if (scaleState.connected) {
                if (scaleState.stable) {
                    text.textContent = scaleState.port
                        ? `${tr('scale_online')} · ${scaleState.port}`
                        : tr('scale_online');
                } else {
                    text.textContent = tr('scale_unstable');
                }
            } else {
                text.textContent = tr('scale_offline');
            }
        }
        const displayW = this.pending ? this.pending.weightKg : this.getManualWeight();
        this.renderSelected();
        this.renderCalc(displayW, !!this.pending);
        this.updateManualHint();
    },

    async connect() {
        const port = document.getElementById('scalePortSelect')?.value || '';
        const baudRate = Number(document.getElementById('scaleBaudSelect')?.value || 9600);
        const autoConnect = !!document.getElementById('scaleAutoConnect')?.checked;
        try {
            await api('/api/scale/config', {
                method: 'POST',
                body: JSON.stringify({ portName: port, baudRate, autoConnect, defaultUnit: 'kg' })
            });
            await api('/api/scale/connect', {
                method: 'POST',
                body: JSON.stringify({ port, baudRate })
            });
            toast(tr('scale_connected'), 'success');
            await this.refreshStatus();
        } catch (e) {
            toast(e.message || tr('scale_connect_failed'), 'error');
        }
    },

    async disconnect() {
        try {
            await api('/api/scale/disconnect', { method: 'POST', body: '{}' });
            toast(tr('scale_disconnected'), 'success');
            await this.refreshStatus();
        } catch (e) { toast(e.message || tr('scale_api_unavailable'), 'error'); }
    },

    async tare() {
        try {
            await api('/api/scale/tare', { method: 'POST', body: '{}' });
            this.pending = null;
            await this.refreshStatus();
        } catch (e) { toast(e.message || tr('scale_api_unavailable'), 'error'); }
    },

    async zero() {
        try {
            await api('/api/scale/zero', { method: 'POST', body: '{}' });
            this.pending = null;
            await this.refreshStatus();
        } catch (e) { toast(e.message || tr('scale_api_unavailable'), 'error'); }
    },

    async requestWeight() {
        const product = this.getSelectedProduct();
        if (!product) {
            toast(tr('weigh_need_product'), 'error');
            return;
        }
        try {
            this.manualMode = false;
            const data = await api('/api/scale/request', { method: 'POST', body: '{}' });
            this.applyState({ ...scaleState, ...data, connected: scaleState.connected || !!data.success });
            let weightKg = Number(scaleState.weight || 0);
            if ((scaleState.unit || 'kg').toLowerCase() === 'g') weightKg = weightKg / 1000;
            this.setManualWeight(weightKg);
            if (weightKg <= 0) {
                this.pending = null;
                toast(tr('weigh_need_scale'), 'error');
                this.render();
                return;
            }
            this.commitPendingFromWeight(weightKg, 'scale');
            this.render();
            toast(tr('scale_weighed_toast')
                .replace('{0}', product.name)
                .replace('{1}', weightKg.toFixed(3))
                .replace('{2}', tr('unit_kg'))
                .replace('{3}', money(this.pending.linePrice)), 'success');
        } catch (e) {
            // Scale failed — fall back to whatever is typed in the manual field
            this.manualMode = true;
            const w = this.getManualWeight();
            if (w > 0) {
                this.commitPendingFromWeight(w, 'manual');
                this.render();
                toast(tr('scale_weighed_toast')
                    .replace('{0}', product.name)
                    .replace('{1}', w.toFixed(3))
                    .replace('{2}', tr('unit_kg'))
                    .replace('{3}', money(this.pending.linePrice)), 'success');
            } else {
                toast(tr('scale_offline_manual'), 'info');
                document.getElementById('scaleManualWeight')?.focus();
            }
        }
    },

    async simulate(weight = 0.525) {
        try {
            await api('/api/scale/simulate', {
                method: 'POST',
                body: JSON.stringify({ weight, unit: 'kg', stable: true })
            });
            toast(tr('scale_simulated').replace('{0}', String(weight)).replace('{1}', tr('unit_kg')), 'success');
            this.manualMode = false;
            await this.refreshStatus();
            const product = this.getSelectedProduct();
            this.setManualWeight(weight);
            if (product) {
                this.commitPendingFromWeight(weight, 'scale');
                this.render();
            }
        } catch (e) {
            // No scale API — still allow Sim as manual fill
            this.manualMode = true;
            this.setManualWeight(weight);
            this.onManualWeightInput();
            toast(tr('scale_manual_set').replace('{0}', String(weight)).replace('{1}', tr('unit_kg')), 'success');
        }
    },

    async addWeighedProduct() {
        const product = this.getSelectedProduct();
        if (!product) {
            toast(tr('weigh_need_product'), 'error');
            return;
        }
        // Prefer pending; otherwise use typed manual weight
        let weight = this.pending?.productId === product.id ? this.pending.weightKg : this.getManualWeight();
        if (!(weight > 0)) {
            toast(tr('weigh_need_read'), 'error');
            document.getElementById('scaleManualWeight')?.focus();
            return;
        }

        const requestedStockGrams = Math.max(1, Math.round(weight * 1000));
        const availableStockGrams = Number(product.stock) || 0;
        const inCartStockGrams = cart
            .filter(x => x.id === product.id && !x.skipStock)
            .reduce((sum, x) => sum + (x.stockQty != null ? Number(x.stockQty) : Math.round((Number(x.weightKg) || Number(x.qty) || 0) * 1000)), 0);

        let skipStock = !isPosProductAvailable(product);

        if (product.isStockTracked !== false && !product.isService && product.itemType !== 'Service' && !skipStock) {
            const remainingGrams = Math.max(0, availableStockGrams - inCartStockGrams);
            if (requestedStockGrams > remainingGrams) {
                const reqKg = weight.toFixed(3);
                const availKg = (remainingGrams / 1000).toFixed(3);
                const confirmed = await showOutOfStockWeightPopup(product.name, reqKg, availKg);
                if (!confirmed) {
                    const input = document.getElementById('scaleManualWeight');
                    if (input) {
                        input.focus();
                        input.select();
                    }
                    return;
                }
                skipStock = true;
            }
        }

        const linePrice = this.calcLinePrice(product, weight);
        const stockQty = skipStock ? 0 : requestedStockGrams;
        const ok = addToCart(product.id, 1, {
            name: `${product.name} (${weight.toFixed(3)} ${tr('unit_kg')})`,
            price: linePrice,
            lineKey: `${product.id}-w-${Date.now()}`,
            weighted: true,
            weightKg: weight,
            stockQty,
            skipStock,
            allowZeroStock: skipStock,
            silentStockError: true
        });
        if (ok === false) return;
        toast(tr('weigh_added').replace('{0}', product.name).replace('{1}', weight.toFixed(3)), 'success');
        this.pending = null;
        this.setManualWeight(0);
        this.render();
        renderCart();
    },

    async resolveBarcode(code) {
        try {
            const res = await fetch('/api/scale/resolve-barcode', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ barcode: code })
            });
            if (res.status === 404) {
                const err = await res.json().catch(() => ({}));
                if (err.isScaleBarcode) {
                    toast(err.error || 'Scale PLU not found', 'error');
                    return true;
                }
                return false;
            }
            if (!res.ok) return false;
            const data = await res.json();
            if (!data.isScaleBarcode) return false;
            const p = data.product;
            const line = data.line;
            if (!products.some(x => x.id === p.id)) {
                products.push({
                    id: p.id, name: p.name, price: p.price, stock: p.stock,
                    isService: p.isService, barcode: p.barcode, sku: p.sku,
                    sellByWeight: true
                });
            }
            addToCart(p.id, line.qty || 1, {
                name: line.name,
                price: line.price,
                lineKey: `${p.id}-scale-${code}-${Date.now()}`,
                weighted: true,
                weightKg: line.weightKg || data.weightKg || 0,
                stockQty: line.stockQty || 0
            });
            toast(line.name, 'success');
            return true;
        } catch {
            return false;
        }
    }
};

document.addEventListener('DOMContentLoaded', async () => {
    try {
        applyI18n();
        setupChrome();
        try { await loadLicense(); } catch (e) { console.error('loadLicense boot error', e); }
        setupNavigation();
        setupAuth();
        try { setupActions(); } catch (e) { console.error('setupActions', e); }
        try { setupTools(); } catch (e) { console.error('setupTools', e); }
        try { setupSignalR(); } catch (e) { console.error('setupSignalR', e); }
        try { wirePrintSettingsUi(); } catch (e) { console.error('wirePrintSettingsUi', e); }
        try { await loadPrintSettings(); } catch { /* ignore */ }

        document.getElementById('feature-scale-toggle')?.addEventListener('change', async (e) => {
            if (!isSoftioSuperAdmin()) {
                e.target.checked = featureFlags.scaleEnabled;
                toast(tr('feature_scale_denied'), 'error');
                return;
            }
            const enabled = !!e.target.checked;
            try {
                const res = await api('/api/features/scale', {
                    method: 'PUT',
                    body: JSON.stringify({
                        enabled,
                        username: currentUser?.username || ''
                    })
                });
                applyFeatureFlags({
                    scaleEnabled: !!res.scaleEnabled,
                    quickSaleEnabled: !!(res.quickSaleEnabled ?? featureFlags.quickSaleEnabled)
                });
                toast(tr('feature_scale_saved'), 'success');
                try { await loadData(); renderAll?.(); } catch { /* ignore */ }
            } catch (err) {
                e.target.checked = featureFlags.scaleEnabled;
                toast(err.message || tr('feature_scale_denied'), 'error');
            }
        });

        document.getElementById('feature-quicksale-toggle')?.addEventListener('change', async (e) => {
            if (!isSoftioSuperAdmin()) {
                e.target.checked = featureFlags.quickSaleEnabled;
                toast(tr('feature_quicksale_denied'), 'error');
                return;
            }
            const enabled = !!e.target.checked;
            try {
                const res = await api('/api/features/quicksale', {
                    method: 'PUT',
                    body: JSON.stringify({
                        enabled,
                        username: currentUser?.username || ''
                    })
                });
                applyFeatureFlags({
                    scaleEnabled: !!(res.scaleEnabled ?? featureFlags.scaleEnabled),
                    quickSaleEnabled: !!res.quickSaleEnabled
                });
                toast(tr('feature_quicksale_saved'), 'success');
            } catch (err) {
                e.target.checked = featureFlags.quickSaleEnabled;
                toast(err.message || tr('feature_quicksale_denied'), 'error');
            }
        });

        const saved = sessionStorage.getItem('generic_user')  ;
        if (saved) {
            try {
                currentUser = JSON.parse(saved);
                applyFeatureFlags(currentUser.features || { scaleEnabled: false, quickSaleEnabled: false });
                showApp();
                await loadFeatureFlags();
                await loadData();
            } catch {


                sessionStorage.removeItem('generic_user');
            }
        }
    } catch (e) {
        console.error('boot failed', e);
        toast('UI failed to start: ' + e.message, 'error');
    } finally {
        // Paint one frame with styles applied, then reveal (host hides splash)
        requestAnimationFrame(() => {
            requestAnimationFrame(() => {
                document.body.classList.add('ui-ready');
                try {
                    if (window.chrome?.webview?.postMessage)
                        window.chrome.webview.postMessage(JSON.stringify({ action: 'uiReady' }));
                } catch { }
            });
        });
    }
});

window.showApp = showApp;
window.hideApp = hideApp;
window.loadData = loadData;
window.applyI18n = applyI18n;
