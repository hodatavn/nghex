using Nghex.Data.Setup;

namespace Nghex.Identity.Setup;

public class AccessPolicyTableScript : IDbTableScript
{
    public IEnumerable<string> GetTableStatements() =>
    [
        """
        CREATE TABLE SYS_ACCESS_POLICY (
            ACCOUNT_ID NUMBER(19) NOT NULL,
            POLICY_TYPE VARCHAR2(50) NOT NULL,
            AP_MODE VARCHAR2(20) DEFAULT 'ALL' NOT NULL,
            CREATED_AT DATE DEFAULT SYSDATE NOT NULL,
            UPDATED_AT DATE,
            CONSTRAINT PK_SYS_ACCESS_POLICY PRIMARY KEY (ACCOUNT_ID, POLICY_TYPE),
            CONSTRAINT FK_SYS_ACCESS_POLICY_ACCOUNT FOREIGN KEY (ACCOUNT_ID) REFERENCES SYS_Accounts (Id),
            CONSTRAINT CHK_SYS_ACCESS_POLICY_MODE CHECK (AP_MODE IN ('ALL', 'RESTRICTED'))
        )
        """,

        """
        CREATE TABLE SYS_ACCESS_POLICY_DETAIL (
            ACCOUNT_ID NUMBER(19) NOT NULL,
            POLICY_TYPE VARCHAR2(50) NOT NULL,
            AP_CODE VARCHAR2(50) NOT NULL,
            CONSTRAINT PK_SYS_ACCESS_POLICY_DETAIL PRIMARY KEY (ACCOUNT_ID, POLICY_TYPE, AP_CODE),
            CONSTRAINT FK_SYS_AP_DETAIL_POLICY FOREIGN KEY (ACCOUNT_ID, POLICY_TYPE)
                REFERENCES SYS_ACCESS_POLICY (ACCOUNT_ID, POLICY_TYPE)
        )
        """,

        "CREATE INDEX idx_SYS_access_policy_account ON SYS_ACCESS_POLICY (ACCOUNT_ID)",
        "CREATE INDEX idx_SYS_ap_detail_account ON SYS_ACCESS_POLICY_DETAIL (ACCOUNT_ID)",
        "CREATE INDEX idx_SYS_ap_detail_policy ON SYS_ACCESS_POLICY_DETAIL (ACCOUNT_ID, POLICY_TYPE)",
    ];

    public IEnumerable<string> GetSeedStatements() => [];
}
