-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[TransaccionesModificar]
	@Id int,
	@UsuarioId int,
	@FechaTransaccion date,
	@Monto decimal (18,2),
	@MontoAnterior decimal (18,2),
	@CuentaId int,
	@CuentaIdAnterior int,
	@CategoriaId int,
	@Nota nvarchar(1000) = null
AS
BEGIN
	DECLARE @Error int;
	DECLARE @rowcount int;

	BEGIN TRAN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	--SET NOCOUNT ON;

	UPDATE Cuentas set balance-=@MontoAnterior where id=@CuentaIdAnterior;

    -- Insert statements for procedure here
	UPDATE TRANSACCIONES SET FechaTransaccion=@FechaTransaccion, Monto=abs(@Monto),
		Nota=@Nota, CuentaId=@CuentaId, CategoriaId=@CategoriaId WHERE Id=@Id AND UsuarioId=@UsuarioId;
	SET @Error=@@ERROR;
	IF (@Error<>0) GOTO TratarError;

	set @rowcount=@@rowcount;

	UPDATE Cuentas set balance+=@Monto where id=@CuentaId;

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
