-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[TransaccionesBorrar]
	@Id int
AS
BEGIN
	DECLARE @Error int;
	DECLARE @rowcount int;
	DECLARE @Monto decimal (18,2);
	DECLARE @TipoOperacionId int;
	DECLARE @CuentaId int;

	BEGIN TRAN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	--SET NOCOUNT ON;

	select @Monto=Monto, @TipoOperacionId=TipoOperacionId, @CuentaId=CuentaId  FROM TRANSACCIONES T
	INNER JOIN CATEGORIAS C ON T.CategoriaId = C.ID
	WHERE T.Id=@Id;

	IF @TipoOperacionId=2 
		set @Monto = @Monto * -1;

	UPDATE Cuentas set balance-=@Monto where id=@CuentaId;

    -- Insert statements for procedure here
	DELETE FROM TRANSACCIONES WHERE Id=@Id;
	SET @Error=@@ERROR;
	IF (@Error<>0) GOTO TratarError;

	set @rowcount=@@rowcount;

	COMMIT TRAN;

	SELECT @rowcount;

	TratarError:
		--Si ha ocurrido algún error llegamos hasta aquí
		If @@Error<>0
			BEGIN
			PRINT 'Ha ocurrido un error. Abortamos la transacción';
			--Se lo comunicamos al usuario y deshacemos la transacción
			--todo volverá a estar como si nada hubiera ocurrido
			ROLLBACK TRAN;
			SELECT 0;
			END
	
END
