function inicializarTransacciones(urlObtenerCategorias)
{
    $('#TipoOperacionId').change(async function () {
        const valorSel = $(this).val();
        const respuesta = await fetch(urlObtenerCategorias,
            {
                method: 'POST',
                body: valorSel,
                headers: {
                    'content-type': 'application/json'
                }
            });

        const json = await respuesta.json();
        const opciones = json.map(categoria => `<option value="${categoria.value}">${categoria.text}</option>`);
        $('#CategoriaId').html(opciones);
    })
}