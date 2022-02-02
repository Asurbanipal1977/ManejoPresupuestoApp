-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE TiposCuentaInsertar
	-- Add the parameters for the stored procedure here
	@Nombre nvarchar(50),
	@UsuarioId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @Orden int;

	--SI ES NULO, ME DARÁ UN CERO
	select @Orden = COALESCE(MAX(Orden),0)+1 FROM TiposCuentas WHERE UsuarioId=@UsuarioId;

    -- Insert statements for procedure here
	INSERT INTO TiposCuentas (Nombre, UsuarioId, Orden) VALUES (@Nombre, @UsuarioId, @Orden);
    SELECT SCOPE_IDENTITY();

END
