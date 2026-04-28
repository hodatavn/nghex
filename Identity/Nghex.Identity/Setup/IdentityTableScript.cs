using Nghex.Data.Setup;

namespace Nghex.Identity.Setup;

public class IdentityTableScript : IDbTableScript
{
    public IEnumerable<string> GetTableStatements() =>
    [
        // Sequences
        "CREATE SEQUENCE SEQ_SYS_ACCOUNTS START WITH 1 INCREMENT BY 1",
        "CREATE SEQUENCE SEQ_SYS_ROLES START WITH 1 INCREMENT BY 1",
        "CREATE SEQUENCE SEQ_SYS_PERMISSIONS START WITH 1 INCREMENT BY 1",
        "CREATE SEQUENCE SEQ_SYS_ACCOUNT_ROLES START WITH 1 INCREMENT BY 1",
        "CREATE SEQUENCE SEQ_SYS_ROLE_PERMISSIONS START WITH 1 INCREMENT BY 1",
        "CREATE SEQUENCE SEQ_SYS_JWT_TOKENS START WITH 1 INCREMENT BY 1",
        "CREATE SEQUENCE SEQ_SYS_MENU_ITEMS START WITH 1 INCREMENT BY 1",
        "CREATE SEQUENCE SEQ_SYS_MENU_ITEM_PERMISSIONS START WITH 1 INCREMENT BY 1",

        // TABLES
        """
        CREATE TABLE SYS_ACCOUNTS (
            ID                      NUMBER(19) DEFAULT SEQ_SYS_ACCOUNTS.NEXTVAL,
            USERNAME                VARCHAR2(100) NOT NULL,
            EMAIL                   VARCHAR2(255) NOT NULL,
            PASSWORD                VARCHAR2(500) NOT NULL,
            DISPLAY_NAME            VARCHAR2(200),
            IP_ADDRESS              VARCHAR2(45),
            IS_ACTIVE               NUMBER(1) DEFAULT 1 NOT NULL,
            IS_LOCKED               NUMBER(1) DEFAULT 0 NOT NULL,
            IS_DELETED              NUMBER(1) DEFAULT 0 NOT NULL,
            LAST_LOGIN_AT           DATE,
            FAILED_LOGIN_ATTEMPTS   NUMBER(10) DEFAULT 0 NOT NULL,
            LOCKED_UNTIL            DATE,
            CREATED_AT              DATE DEFAULT SYSDATE NOT NULL,
            UPDATED_AT              DATE,
            CREATED_BY              VARCHAR2(100),
            UPDATED_BY              VARCHAR2(100),
            CONSTRAINT PK_SYS_ACCOUNTS PRIMARY KEY (ID),
            CONSTRAINT UQ_SYS_ACCOUNTS_USERNAME UNIQUE (USERNAME),
            CONSTRAINT UQ_SYS_ACCOUNTS_EMAIL UNIQUE (EMAIL)
        )
        """,

        """
        CREATE TABLE SYS_ROLES (
            ID              NUMBER(19) DEFAULT SEQ_SYS_ROLES.NEXTVAL,
            NAME            VARCHAR2(100) NOT NULL,
            DESCRIPTION     VARCHAR2(500),
            CODE            VARCHAR2(50) NOT NULL,
            ROLE_LEVEL      NUMBER(2) DEFAULT 99 NOT NULL,
            IS_ACTIVE       NUMBER(1) DEFAULT 1 NOT NULL,
            IS_DELETED      NUMBER(1) DEFAULT 0 NOT NULL,
            CREATED_AT      DATE DEFAULT SYSDATE NOT NULL,
            UPDATED_AT      DATE,
            CREATED_BY      VARCHAR2(100),
            UPDATED_BY      VARCHAR2(100),
        CONSTRAINT PK_SYS_ROLES PRIMARY KEY (ID),
        CONSTRAINT UQ_SYS_ROLES_CODE UNIQUE (CODE)
        )
        """,

        """
        CREATE TABLE SYS_PERMISSIONS (
            ID              NUMBER(19) DEFAULT SEQ_SYS_PERMISSIONS.NEXTVAL,
            CODE            VARCHAR2(100) NOT NULL,
            NAME            VARCHAR2(100) NOT NULL,
            MODULE          VARCHAR2(100),
            PLUGIN_NAME     VARCHAR2(100),
            DESCRIPTION     VARCHAR2(500),
            IS_ACTIVE       NUMBER(1) DEFAULT 1 NOT NULL,
            IS_DELETED      NUMBER(1) DEFAULT 0 NOT NULL,
            CREATED_AT      DATE DEFAULT SYSDATE NOT NULL,
            UPDATED_AT      DATE,
            CREATED_BY      VARCHAR2(100),
            UPDATED_BY      VARCHAR2(100),
        CONSTRAINT PK_SYS_PERMISSIONS PRIMARY KEY (ID),
        CONSTRAINT UQ_SYS_PERMISSIONS_CODE UNIQUE (CODE)
        )
        """,

        """
        CREATE TABLE SYS_ACCOUNT_ROLES (
            ID          NUMBER(19) DEFAULT SEQ_SYS_ACCOUNT_ROLES.NEXTVAL,
            ACCOUNT_ID  NUMBER(19) NOT NULL,
            ROLE_ID     NUMBER(19) NOT NULL,
            CONSTRAINT PK_SYS_ACCOUNT_ROLES PRIMARY KEY (ID),
            CONSTRAINT FK_SYS_ACCOUNT_ROLES_ACCOUNT FOREIGN KEY (ACCOUNT_ID) REFERENCES SYS_ACCOUNTS (ID),
            CONSTRAINT FK_SYS_ACCOUNT_ROLES_ROLE FOREIGN KEY (ROLE_ID) REFERENCES SYS_ROLES (ID),
            CONSTRAINT UQ_SYS_ACCOUNT_ROLES UNIQUE (ACCOUNT_ID, ROLE_ID)
        )
        """,

        """
        CREATE TABLE SYS_ROLE_PERMISSIONS (
            ID              NUMBER(19) DEFAULT SEQ_SYS_ROLE_PERMISSIONS.NEXTVAL,
            ROLE_ID         NUMBER(19) NOT NULL,
            PERMISSION_ID   NUMBER(19) NOT NULL,
            CONSTRAINT PK_SYS_ROLE_PERMISSIONS PRIMARY KEY (ID),
            CONSTRAINT FK_SYS_ROLE_PERMISSIONS_ROLE FOREIGN KEY (ROLE_ID) REFERENCES SYS_ROLES (ID),
            CONSTRAINT FK_SYS_ROLE_PERMISSIONS_PERMISSION FOREIGN KEY (PERMISSION_ID) REFERENCES SYS_PERMISSIONS (ID),
            CONSTRAINT UQ_SYS_ROLE_PERMISSIONS UNIQUE (ROLE_ID, PERMISSION_ID)
        )
        """,

        """
        CREATE TABLE SYS_JWT_TOKENS (
            ID                  NUMBER(19) DEFAULT SEQ_SYS_JWT_TOKENS.NEXTVAL,
            ACCOUNT_ID          NUMBER(19) NOT NULL,
            TOKEN_ID            VARCHAR2(255) NOT NULL,
            REFRESH_TOKEN       VARCHAR2(500) NOT NULL,
            EXPIRES_AT          DATE NOT NULL,
            REFRESH_EXPIRES_AT  DATE NOT NULL,
            IS_REVOKED          NUMBER(1) DEFAULT 0 NOT NULL,
            REVOKED_AT          DATE,
            IP_ADDRESS          VARCHAR2(45),
            USER_AGENT          VARCHAR2(500),
            CREATED_AT          DATE DEFAULT SYSDATE NOT NULL,
            UPDATED_AT          DATE,
            CREATED_BY          VARCHAR2(100),
            UPDATED_BY          VARCHAR2(100),
            CONSTRAINT PK_SYS_JWT_TOKENS PRIMARY KEY (ID),
            CONSTRAINT UQ_SYS_JWT_TOKENS_TOKEN_ID UNIQUE (TOKEN_ID),
            CONSTRAINT FK_JWT_TOKENS_ACCOUNTS FOREIGN KEY (ACCOUNT_ID) REFERENCES SYS_ACCOUNTS (ID)
        )
        """,

        """
        CREATE TABLE SYS_MENU_ITEMS (
            ID                  NUMBER(19) DEFAULT SEQ_SYS_MENU_ITEMS.NEXTVAL,
            MENU_KEY            VARCHAR2(100) NOT NULL,
            PARENT_KEY          VARCHAR2(100),
            TITLE               VARCHAR2(200) NOT NULL,
            ROUTE               VARCHAR2(500),
            ICON                VARCHAR2(200),
            PLUGIN_NAME         VARCHAR2(100),
            PERMISSION_PREFIX   VARCHAR2(100),
            SORT_ORDER          NUMBER(10) DEFAULT 0 NOT NULL,
            IS_ACTIVE           NUMBER(1) DEFAULT 1 NOT NULL,
            CREATED_AT          DATE DEFAULT SYSDATE NOT NULL,
            UPDATED_AT          DATE,
            CREATED_BY          VARCHAR2(100),
            UPDATED_BY          VARCHAR2(100),
            CONSTRAINT PK_SYS_MENU_ITEMS PRIMARY KEY (ID),
            CONSTRAINT UQ_SYS_MENU_ITEMS_KEY UNIQUE (MENU_KEY)
        )
        """,

        """
        CREATE TABLE SYS_MENU_ITEM_PERMISSIONS (
            ID                  NUMBER(19) DEFAULT SEQ_SYS_MENU_ITEM_PERMISSIONS.NEXTVAL,
            MENU_KEY            VARCHAR2(100) NOT NULL,
            PERMISSION_CODE     VARCHAR2(100) NOT NULL,
            CONSTRAINT PK_SYS_MENU_ITEM_PERMISSIONS PRIMARY KEY (ID),
            CONSTRAINT FK_MENU_ITEM_PERM_MENU_KEY FOREIGN KEY (MENU_KEY) REFERENCES SYS_MENU_ITEMS (MENU_KEY)
        )
        """,

        // INDEXES
        "CREATE INDEX IDX_SYS_ACCOUNTS_USERNAME ON SYS_ACCOUNTS (USERNAME)",
        "CREATE INDEX IDX_SYS_ACCOUNTS_EMAIL ON SYS_ACCOUNTS (EMAIL)",
        "CREATE INDEX IDX_SYS_ROLES_CODE ON SYS_ROLES (CODE)",
        "CREATE INDEX IDX_SYS_PERMISSIONS_CODE ON SYS_PERMISSIONS (CODE)",
        "CREATE INDEX IDX_SYS_ACCOUNT_ROLES_ACCOUNT ON SYS_ACCOUNT_ROLES (ACCOUNT_ID)",
        "CREATE INDEX IDX_SYS_ROLE_PERMISSIONS_ROLE ON SYS_ROLE_PERMISSIONS (ROLE_ID)",
        "CREATE INDEX IDX_SYS_JWT_TOKENS_ACCOUNT ON SYS_JWT_TOKENS (ACCOUNT_ID)",
        "CREATE INDEX IDX_SYS_JWT_TOKENS_TOKEN_ID ON SYS_JWT_TOKENS (TOKEN_ID)",
        "CREATE INDEX IDX_SYS_JWT_TOKENS_REFRESH_TOKEN ON SYS_JWT_TOKENS (REFRESH_TOKEN)",
        "CREATE INDEX IDX_SYS_MENU_ITEMS_PARENT ON SYS_MENU_ITEMS (PARENT_KEY)",
        "CREATE INDEX IDX_SYS_MENU_ITEMS_ACTIVE ON SYS_MENU_ITEMS (IS_ACTIVE)",
        "CREATE INDEX IDX_SYS_MENU_ITEM_PERM_MENU ON SYS_MENU_ITEM_PERMISSIONS (MENU_KEY)",
        "CREATE INDEX IDX_SYS_MENU_ITEM_PERM_CODE ON SYS_MENU_ITEM_PERMISSIONS (PERMISSION_CODE)",
        ];

    public IEnumerable<string> GetSeedStatements() =>
    [
        // Roles
        "INSERT INTO SYS_ROLES (NAME, DESCRIPTION, CODE, ROLE_LEVEL) VALUES 	('Super Admin', 'System administrator', 'SUPER_ADMIN', 0)",
        "INSERT INTO SYS_ROLES (NAME, DESCRIPTION, CODE, ROLE_LEVEL) VALUES 	('Admin', 'System administrator', 'ADMIN', 1)",
        "INSERT INTO SYS_ROLES (NAME, DESCRIPTION, CODE, ROLE_LEVEL) VALUES 	('User', 'Normal user', 'USER', 99)",

        // Permissions
        "INSERT INTO SYS_PERMISSIONS (CODE, NAME, MODULE, DESCRIPTION, IS_ACTIVE, IS_DELETED) VALUES 	('ROLE_READ', 'Read Role', 'Role', 'View role', 1, 0)",
        "INSERT INTO SYS_PERMISSIONS (CODE, NAME, MODULE, DESCRIPTION, IS_ACTIVE, IS_DELETED) VALUES 	('ROLE_WRITE', 'Write Role', 'Role', 'Create/edit role', 1, 0)",
        "INSERT INTO SYS_PERMISSIONS (CODE, NAME, MODULE, DESCRIPTION, IS_ACTIVE, IS_DELETED) VALUES 	('ROLE_DELETE', 'Delete Role', 'Role', 'Delete role', 1, 0)",
        "INSERT INTO SYS_PERMISSIONS (CODE, NAME, MODULE, DESCRIPTION, IS_ACTIVE, IS_DELETED) VALUES 	('PERMISSION_READ', 'Read Permission', 'Permission', 'View permission', 1, 0)",
        "INSERT INTO SYS_PERMISSIONS (CODE, NAME, MODULE, DESCRIPTION, IS_ACTIVE, IS_DELETED) VALUES 	('PERMISSION_WRITE', 'Write Permission', 'Permission', 'Create/edit permission', 1, 0)",
        "INSERT INTO SYS_PERMISSIONS (CODE, NAME, MODULE, DESCRIPTION, IS_ACTIVE, IS_DELETED) VALUES 	('PERMISSION_DELETE', 'Delete Permission', 'Permission', 'Delete permission', 1, 0)",
        "INSERT INTO SYS_PERMISSIONS (CODE, NAME, MODULE, DESCRIPTION, IS_ACTIVE, IS_DELETED) VALUES 	('ACCOUNT_READ', 'Read Account', 'Account', 'View account information', 1, 0)",
        "INSERT INTO SYS_PERMISSIONS (CODE, NAME, MODULE, DESCRIPTION, IS_ACTIVE, IS_DELETED) VALUES 	('ACCOUNT_WRITE', 'Write Account', 'Account', 'Create/edit account', 1, 0)",
        "INSERT INTO SYS_PERMISSIONS (CODE, NAME, MODULE, DESCRIPTION, IS_ACTIVE, IS_DELETED) VALUES 	('ACCOUNT_DELETE', 'Delete Account', 'Account', 'Delete account', 1, 0)",
        "INSERT INTO SYS_PERMISSIONS (CODE, NAME, MODULE, DESCRIPTION, IS_ACTIVE, IS_DELETED) VALUES 	('MENU_READ', 'Read Menu', 'Menu', 'View menu', 1, 0)",
        "INSERT INTO SYS_PERMISSIONS (CODE, NAME, MODULE, DESCRIPTION, IS_ACTIVE, IS_DELETED) VALUES 	('MENU_WRITE', 'Write Menu', 'Menu', 'Create/edit menu', 1, 0)",
        "INSERT INTO SYS_PERMISSIONS (CODE, NAME, MODULE, DESCRIPTION, IS_ACTIVE, IS_DELETED) VALUES 	('MENU_DELETE', 'Delete Menu', 'Menu', 'Delete menu', 1, 0)",
        "INSERT INTO SYS_PERMISSIONS (CODE, NAME, MODULE, DESCRIPTION, IS_ACTIVE, IS_DELETED) VALUES 	('LOG_READ', 'Read Log', 'Logging', 'View logs', 1, 0)",
        "INSERT INTO SYS_PERMISSIONS (CODE, NAME, MODULE, DESCRIPTION, IS_ACTIVE, IS_DELETED) VALUES 	('LICENSE_READ', 'Read License', 'License', 'View license', 1, 0)",
        "INSERT INTO SYS_PERMISSIONS (CODE, NAME, MODULE, DESCRIPTION, IS_ACTIVE, IS_DELETED) VALUES 	('CONFIG_READ', 'Read Configuration', 'Configuration', 'View configuration values', 1, 0)",
        "INSERT INTO SYS_PERMISSIONS (CODE, NAME, MODULE, DESCRIPTION, IS_ACTIVE, IS_DELETED) VALUES 	('CONFIG_WRITE', 'Write Configuration', 'Configuration', 'Edit configuration values', 1, 0)",
        "INSERT INTO SYS_PERMISSIONS (CODE, NAME, MODULE, DESCRIPTION, IS_ACTIVE, IS_DELETED) VALUES 	('CONFIG_CREATE', 'Create Configuration', 'Configuration', 'Create new configuration', 1, 0)",
        "INSERT INTO SYS_PERMISSIONS (CODE, NAME, MODULE, DESCRIPTION, IS_ACTIVE, IS_DELETED) VALUES 	('SYSTEM_SETUP', 'Setup System', 'System', 'Setup system / database', 1, 0)",
        "INSERT INTO SYS_PERMISSIONS (CODE, NAME, MODULE, DESCRIPTION, IS_ACTIVE, IS_DELETED) VALUES 	('PLUGIN_READ', 'Read Plugin', 'Plugin', 'View plugins', 1, 0)",
        "INSERT INTO SYS_PERMISSIONS (CODE, NAME, MODULE, DESCRIPTION, IS_ACTIVE, IS_DELETED) VALUES 	('PLUGIN_MANAGE', 'Manage Plugin', 'Plugin', 'Manage plugins', 1, 0)",
        
        // Role permissions — SUPER_ADMIN gets all
        """
        INSERT INTO SYS_ROLE_PERMISSIONS (ROLE_ID, PERMISSION_ID)
        SELECT R.ID, P.ID FROM SYS_ROLES R, SYS_PERMISSIONS P
        WHERE R.CODE = 'SUPER_ADMIN'
        """,

        // Role permissions — ADMIN
        """
        INSERT INTO SYS_ROLE_PERMISSIONS (ROLE_ID, PERMISSION_ID)
        SELECT R.ID, P.ID FROM SYS_ROLES R, SYS_PERMISSIONS P
        WHERE R.CODE = 'ADMIN'
            AND P.CODE IN ('LOG_READ','LICENSE_READ','MENU_READ','ROLE_READ','ROLE_WRITE','ROLE_DELETE',
                        'PERMISSION_READ','ACCOUNT_READ','ACCOUNT_WRITE','ACCOUNT_DELETE', 'CONFIG_READ','CONFIG_WRITE', 'SYSTEM_SETUP')
        """,

        // Role permissions — USER
        """
        INSERT INTO SYS_ROLE_PERMISSIONS (ROLE_ID, PERMISSION_ID)
        SELECT R.ID, P.ID FROM SYS_ROLES R, SYS_PERMISSIONS P
        WHERE R.CODE = 'USER' AND P.CODE LIKE '%PATIENT%'
        """,

        // Accounts — password: admin123
        """
        INSERT INTO SYS_ACCOUNTS (USERNAME, EMAIL, PASSWORD, DISPLAY_NAME, IS_ACTIVE, CREATED_BY) 
        VALUES ('system', 'system@data-hub.com', '8AHtbeTKtkL/mYNhAi8LksjfUCQb3k3vhIQVZIgcNbIgoG5LqQT2GdolSUlKZWGX', 'System Account', 1, 'system')
        """,
        """
        INSERT INTO SYS_ACCOUNT_ROLES (ACCOUNT_ID, ROLE_ID)
        SELECT A.ID, R.ID FROM SYS_ACCOUNTS A, SYS_ROLES R
        WHERE A.USERNAME = 'system' AND R.CODE = 'SUPER_ADMIN'
        """,

        """
        INSERT INTO SYS_ACCOUNTS (USERNAME, EMAIL, PASSWORD, DISPLAY_NAME, IS_ACTIVE, CREATED_BY) 
        VALUES ('admin', 'admin@data-hub.com', '8AHtbeTKtkL/mYNhAi8LksjfUCQb3k3vhIQVZIgcNbIgoG5LqQT2GdolSUlKZWGX', 'Administrator', 1, 'system')
        """,
        """
        INSERT INTO SYS_ACCOUNT_ROLES (ACCOUNT_ID, ROLE_ID)
        SELECT A.ID, R.ID FROM SYS_ACCOUNTS A, SYS_ROLES R
        WHERE A.USERNAME = 'admin' AND R.CODE = 'ADMIN'
        """,

        // Menu items
        """
        INSERT INTO SYS_MENU_ITEMS (MENU_KEY, PARENT_KEY, TITLE, ROUTE, ICON, SORT_ORDER, IS_ACTIVE, CREATED_AT, CREATED_BY) 
        VALUES ('dashboard', NULL, 'Dashboard', NULL, 'dashboard', 0, 1, SYSDATE, 'system')
        """,
        """
        INSERT INTO SYS_MENU_ITEMS (MENU_KEY, PARENT_KEY, TITLE, ROUTE, ICON, SORT_ORDER, IS_ACTIVE, CREATED_AT, CREATED_BY) 
        VALUES ('setup', NULL, 'Setup hệ thống', NULL, 'settings', 10, 1, SYSDATE, 'system')
        """,
        """
        INSERT INTO SYS_MENU_ITEMS (MENU_KEY, PARENT_KEY, TITLE, ROUTE, ICON, SORT_ORDER, IS_ACTIVE, CREATED_AT, CREATED_BY) 
        VALUES ('admin', NULL, 'Quản trị', NULL, 'admin_panel_settings', 20, 1, SYSDATE, 'system')
        """,
        """
        INSERT INTO SYS_MENU_ITEMS (MENU_KEY, PARENT_KEY, TITLE, ROUTE, ICON, SORT_ORDER, IS_ACTIVE, CREATED_AT, CREATED_BY) 
        VALUES ('admin.managements', NULL, 'Quản lý quyền và tài khoản', NULL, 'setting', 4, 1, SYSDATE, 'system')
        """,
        """
        INSERT INTO SYS_MENU_ITEMS (MENU_KEY, PARENT_KEY, TITLE, ROUTE, ICON, SORT_ORDER, IS_ACTIVE, CREATED_AT, CREATED_BY) 
        VALUES ('setup.system', 'setup', 'Hệ thống', '/setup/system', 'dns', 10, 1, SYSDATE, 'system')
        """,
        """
        INSERT INTO SYS_Menu_Items (Menu_Key, Parent_Key, Title, Route, Icon, Permission_Prefix, Sort_Order, Is_Active, Created_At, Created_By) 
        VALUES ('setup.jwt', 'setup', 'JWT', '/setup/jwt', 'key', 'jwt', 20, 1, SYSDATE, 'system')
        """,
        """
        INSERT INTO SYS_Menu_Items (Menu_Key, Parent_Key, Title, Route, Icon, Permission_Prefix, Sort_Order, Is_Active, Created_At, Created_By) 
        VALUES ('setup.plugin', 'setup', 'Plugin', '/setup/plugins', 'plugin', 'plugin', 30, 1, SYSDATE, 'system')
        """,
        """
        INSERT INTO SYS_Menu_Items (Menu_Key, Parent_Key, Title, Route, Icon, Permission_Prefix, Sort_Order, Is_Active, Created_At, Created_By) 
        VALUES ('setup.menu', 'setup', 'Quản lý menu', '/admin/menu', 'menu', 'MENU', 60, 1, SYSDATE, 'system')
        """,
        """
        INSERT INTO SYS_Menu_Items (Menu_Key, Parent_Key, Title, Route, Icon, Permission_Prefix, Sort_Order, Is_Active, Created_At, Created_By) 
        VALUES ('admin.logs', 'admin', 'Log', '/admin/logs', 'article', 'LOG', 10, 1, SYSDATE, 'system')
        """,
        """
        INSERT INTO SYS_Menu_Items (Menu_Key, Parent_Key, Title, Route, Icon, Permission_Prefix, Sort_Order, Is_Active, Created_At, Created_By) 
        VALUES ('admin.license', 'admin', 'License', '/admin/license', 'verified', 'LICENSE', 20, 1, SYSDATE, 'system')
        """,
        """
        INSERT INTO SYS_Menu_Items (Menu_Key, Parent_Key, Title, Route, Icon, Sort_Order, Is_Active, Created_At, Created_By) 
        VALUES ('admin.uac', 'admin', 'Phân quyền sử dụng', '/admin/uac', 'set_permission', 4, 1, SYSDATE, 'system')
        """,
        """
        INSERT INTO SYS_Menu_Items (Menu_Key, Parent_Key, Title, Route, Icon, Sort_Order, Is_Active, Created_At, Created_By) 
        VALUES ('admin.configurations', 'admin', 'Configuration', '/admin/configurations', 'tune', 40, 1, SYSDATE, 'system')
        """,
        """
        INSERT INTO SYS_Menu_Items (Menu_Key, Parent_Key, Title, Route, Icon, Permission_Prefix, Sort_Order, Is_Active, Created_At, Created_By) 
        VALUES ('admin.accounts', 'admin.managements', 'Accounts', '/admin/accounts', 'people', 'ACCOUNT', 30, 1, SYSDATE, 'system')
        """,
        """        
        INSERT INTO SYS_Menu_Items (Menu_Key, Parent_Key, Title, Route, Icon, Permission_Prefix, Sort_Order, Is_Active, Created_At, Created_By) 
        VALUES ('setup.roles', 'admin.managements', 'Roles', '/admin/roles', 'security', 'ROLE', 60, 1, SYSDATE, 'system')
        """,
        """
        INSERT INTO SYS_Menu_Items (Menu_Key, Parent_Key, Title, Route, Icon, Permission_Prefix, Sort_Order, Is_Active, Created_At, Created_By) 
        VALUES ('setup.permissions', 'admin.managements', 'Permissions', '/admin/permissions', 'security', 'PERMISSION', 70, 1, SYSDATE, 'system')
        """,

        // Menu item permissions
        "INSERT INTO SYS_MENU_ITEM_PERMISSIONS (MENU_KEY, PERMISSION_CODE) VALUES ('admin.logs', 'LOG_READ')",
        "INSERT INTO SYS_MENU_ITEM_PERMISSIONS (MENU_KEY, PERMISSION_CODE) VALUES ('admin.license', 'LICENSE_READ')",
        "INSERT INTO SYS_MENU_ITEM_PERMISSIONS (MENU_KEY, PERMISSION_CODE) VALUES ('admin.uac', 'PERMISSION_READ')",
        "INSERT INTO SYS_MENU_ITEM_PERMISSIONS (MENU_KEY, PERMISSION_CODE) VALUES ('admin.configurations', 'CONFIG_READ')",
        "INSERT INTO SYS_MENU_ITEM_PERMISSIONS (MENU_KEY, PERMISSION_CODE) VALUES ('admin.accounts', 'ACCOUNT_READ')",
        "INSERT INTO SYS_MENU_ITEM_PERMISSIONS (MENU_KEY, PERMISSION_CODE) VALUES ('admin.managements', 'ROLE_READ')",
        "INSERT INTO SYS_MENU_ITEM_PERMISSIONS (MENU_KEY, PERMISSION_CODE) VALUES ('setup.roles', 'ROLE_READ')",
        "INSERT INTO SYS_MENU_ITEM_PERMISSIONS (MENU_KEY, PERMISSION_CODE) VALUES ('setup.permissions', 'PERMISSION_READ')",
        "INSERT INTO SYS_MENU_ITEM_PERMISSIONS (MENU_KEY, PERMISSION_CODE) VALUES ('setup.plugin', 'PLUGIN_MANAGE')",
        "INSERT INTO SYS_MENU_ITEM_PERMISSIONS (MENU_KEY, PERMISSION_CODE) VALUES ('setup.menu', 'MENU_READ')",
        "COMMIT",
    ];
}
