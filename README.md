# Monitorador De Ativos
Uma ferramenta CLI que indica se um ativo sair de um determinado intervalo, avisando o usuário por e-mail. Programada como parte de um processo seletivo.

## Utilidade
Serviria como lembrete para a compra ou venda de ações que cairam ou subiram de preço.

## Como usar
* Para compilar, utilize IDE ou CMD conforme preferência. (O código foi desenvolvido no visual studio, portanto contém incluso uma solução .sln)
* Além disso é necessário configurar variáveis de ambiente. São necessários 6 valores do user secrets do .NET.
  * source_email_name : Nome de usuário que envia o e-mail
  * source_email_email : Email do usuário que envia e-mail
  * source_email_key : Chave de API da sendgrid válida
  * dest_email_name : Nome de usuário que recebe o e-mail
  * dest_email_email : Email do usuário que recebe o e-mail
  * asset_api_key : Chave de API da alphavantage válida
