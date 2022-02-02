-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[TransaccionesInsertar]
	@UsuarioId int,
	@FechaTransaccion date,
	@Monto decimal (18,2),
	@Nota nvarchar(1000) = null,
	@CuentaId int,
	@CategoriaId int

AS
BEGIN
	DECLARE @Error int;

	BEGIN TRAN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	INSERT INTO TRANSACCIONES (UsuarioId, FechaTransaccion, Monto, Nota, CuentaId, CategoriaId) VALUES 
	(@UsuarioId, @FechaTransaccion, abs(@Monto), @Nota, @CuentaId, @CategoriaId);
	SET @Error=@@ERROR;
	IF (@Error<>0) GOTO TratarError;

	UPDATE Cuentas set balance+=@Monto where id=@CuentaId;
	COMMIT TRAN;

	select SCOPE_IDENTITY();

	TratarError:
		--Si ha ocurrido algún error llegamos hasta aquí
		If @@Error<>0
			BEGIN
			PRINT 'Ha ocurrido un error. Abortamos la transacción';
			--Se lo comunicamos al usuario y deshacemos la transacción
			--todo volverá a estar como si nada hubiera ocurrido
			ROLLBACK TRAN;
			select 0;
			END
	
END
