create or replace PROCEDURE USP_GET_ENVIRONMENT_DATA(IN_VIN           IN VARCHAR2,
                                                     IN_MAKE_DATE     IN DATE,
                                                     OUT_DATA_SET     OUT sys_refcursor,
                                                     ERROR_MESSAGE    OUT VARCHAR2) AS

BEGIN
    ERROR_MESSAGE := 'NO ERROR';
    open OUT_DATA_SET for select * from dyno_param;
END;