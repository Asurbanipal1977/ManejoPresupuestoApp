-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[CrearDatosPorDefecto]
	-- Add the parameters for the stored procedure here
	@UsuarioId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @Efectivo nvarchar(50) = 'Efectivo';
	DECLARE @CuentasDeBanco nvarchar(50) = 'Cuentas de Banco';
	DECLARE @Tarjetas nvarchar(50) = 'Tarjetas';
	DECLARE @Error int;

	BEGIN TRAN [Tran1]
	  BEGIN TRY
		INSERT INTO TiposCuentas (Nombre,UsuarioId,Orden) VALUES
			(@Efectivo,@UsuarioId,1), (@CuentasDeBanco,@UsuarioId,2), (@Tarjetas,@UsuarioId,3);

		INSERT INTO Cuentas (Nombre, Balance, TipoCuentaId)
		SELECT Nombre, 0, Id FROM TiposCuentas WHERE UsuarioId=@UsuarioId;

		INSERT INTO Categorias (Nombre,TipoOperacionId,UsuarioId) values
		('Libros',2,@UsuarioId), 
		('Salario',1,@UsuarioId),  
		('Comida',2,@UsuarioId),
		('Ventas',1,@UsuarioId);
		  
		COMMIT TRAN [Tran1]
	  END TRY
	  BEGIN CATCH
		  ROLLBACK TRAN [Tran1]
	  END CATCH
END
