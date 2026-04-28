using Nghex.Data.Setup;

namespace Nghex.Configuration.Setup;

public class ConfigurationTableScript : IDbTableScript
{
    public IEnumerable<string> GetTableStatements() =>
    [
        "CREATE SEQUENCE seq_sys_configurations START WITH 1 INCREMENT BY 1",

        """
        CREATE TABLE SYS_Configurations (
            Id NUMBER(19) DEFAULT seq_sys_configurations.NEXTVAL,
            Key VARCHAR2(200) NOT NULL,
            Value VARCHAR2(4000),
            Description VARCHAR2(1000),
            Data_Type VARCHAR2(50) DEFAULT 'string' NOT NULL,
            Module VARCHAR2(100),
            Is_System_Config NUMBER(1) DEFAULT 0 NOT NULL,
            Is_Editable NUMBER(1) DEFAULT 1 NOT NULL,
            Default_Value VARCHAR2(4000),
            Is_Active NUMBER(1) DEFAULT 1 NOT NULL,
            Created_By VARCHAR2(100),
            Created_At DATE DEFAULT SYSDATE NOT NULL,
            Updated_By VARCHAR2(100),
            Updated_At DATE,
            CONSTRAINT PK_SYS_Configurations PRIMARY KEY (Id),
            CONSTRAINT UQ_SYS_configurations_key UNIQUE (Key)
        )
        """,

        "CREATE INDEX idx_SYS_configurations_key ON SYS_Configurations (Key)",
        "CREATE INDEX idx_SYS_configurations_module ON SYS_Configurations (Module)",
        "CREATE INDEX idx_SYS_configurations_is_system ON SYS_Configurations (Is_System_Config)",
    ];

    public IEnumerable<string> GetSeedStatements() =>
    [
        "INSERT INTO SYS_Configurations (Key, Value, Description, Data_Type, Module, Is_System_Config, Is_Editable, Created_By) VALUES ('LOG_RETENTION_DAYS', '30', 'Log retention period in days', 'int', 'Logging', 1, 1, 'system')",
        "INSERT INTO SYS_Configurations (Key, Value, Description, Data_Type, Module, Is_System_Config, Is_Editable, Created_By) VALUES ('MAX_LOGIN_ATTEMPTS', '5', 'Maximum number of failed login attempts', 'int', 'Account', 1, 1, 'system')",
        "INSERT INTO SYS_Configurations (Key, Value, Description, Data_Type, Module, Is_System_Config, Is_Editable, Created_By) VALUES ('LOGIN_FAILED_ATTEMPTS_LOCKOUT_DURATION', '30', 'Account lockout duration in minutes', 'int', 'Account', 1, 1, 'system')",
        "COMMIT",
    ];
}
