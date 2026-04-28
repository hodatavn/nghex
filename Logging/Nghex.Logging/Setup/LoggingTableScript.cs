using Nghex.Data.Setup;

namespace Nghex.Logging.Setup;

public class LoggingTableScript : IDbTableScript
{
    public IEnumerable<string> GetTableStatements() =>
    [
        "CREATE SEQUENCE seq_sys_logs START WITH 1 INCREMENT BY 1",

        """
        CREATE TABLE SYS_Logs (
            Id NUMBER(19) DEFAULT seq_sys_logs.NEXTVAL,
            Log_Level NUMBER(10) NOT NULL,
            Message VARCHAR2(4000) NOT NULL,
            Class_Name VARCHAR2(500),
            Line_Number NUMBER(10),
            Details CLOB,
            Source VARCHAR2(500),
            User_Id NUMBER(19),
            Username VARCHAR2(100),
            Ip_Address VARCHAR2(45),
            User_Agent VARCHAR2(1000),
            Request_Id VARCHAR2(100),
            Module VARCHAR2(100),
            Action VARCHAR2(200),
            Log_Exception CLOB,
            Stack_Trace CLOB,
            Created_By VARCHAR2(100),
            Created_At DATE DEFAULT SYSDATE NOT NULL,
            CONSTRAINT PK_SYS_Logs PRIMARY KEY (Id)
        )
        """,

        "CREATE INDEX idx_SYS_logs_level ON SYS_Logs (Log_Level)",
        "CREATE INDEX idx_SYS_logs_created_at ON SYS_Logs (Created_At)",
        "CREATE INDEX idx_SYS_logs_user_id ON SYS_Logs (User_Id)",
        "CREATE INDEX idx_SYS_logs_module ON SYS_Logs (Module)",
        "CREATE INDEX idx_SYS_logs_request_id ON SYS_Logs (Request_Id)",
    ];

    public IEnumerable<string> GetSeedStatements() => [];
}
