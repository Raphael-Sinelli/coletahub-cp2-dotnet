using ColetaHub.Web.Models;

namespace ColetaHub.Web.Repositories;

/// <summary>
/// Dados iniciais do ponto ColetaHub Vila Mariana (São Paulo/SP). As datas são relativas ao dia
/// em que a aplicação sobe, então sempre existem pacotes recentes e pacotes parados há mais de 7 dias.
/// Cobre todos os status, todos os marketplaces e inclui pacotes frágeis.
/// </summary>
public static class PacoteSeed
{
    public static IReadOnlyList<Pacote> Criar()
    {
        // Unspecified: mesma forma das datas vindas dos formulários e da API ("2026-10-20T00:00:00", sem fuso).
        var hoje = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Unspecified);

        return
        [
            new Pacote
            {
                CodigoRastreio = "QB482917365BR", NomeDestinatario = "Ana Beatriz Ferreira Lima", CpfDestinatario = "428.193.057-41",
                Marketplace = "Mercado Livre", PesoKg = 1.25m, ValorDeclarado = 189.90m, DataRecebimento = hoje.AddDays(-2),
                Status = StatusPacote.AguardandoRetirada, TipoOperacao = TipoOperacao.RetiradaCliente,
                Observacoes = "Cliente avisada por WhatsApp. Mora na Rua Domingos de Morais."
            },
            new Pacote
            {
                CodigoRastreio = "OY731045286BR", NomeDestinatario = "Lucas Henrique Almeida", CpfDestinatario = "315.762.094-07",
                Marketplace = "Shopee", PesoKg = 0.48m, ValorDeclarado = 59.90m, DataRecebimento = hoje.AddDays(-10),
                Status = StatusPacote.AguardandoRetirada, TipoOperacao = TipoOperacao.RetiradaCliente,
                Observacoes = "Segunda notificação enviada. Prazo de guarda de 15 dias vence em breve."
            },
            new Pacote
            {
                CodigoRastreio = "NL205839174BR", NomeDestinatario = "Mariana Costa Ribeiro", CpfDestinatario = "507.284.163-44",
                Marketplace = "Amazon", PesoKg = 3.80m, ValorDeclarado = 1299.00m, DataRecebimento = hoje.AddDays(-1),
                Status = StatusPacote.Recebido, TipoOperacao = TipoOperacao.RetiradaCliente, Fragil = true,
                Observacoes = "Monitor de 27 polegadas. Guardar na prateleira baixa."
            },
            new Pacote
            {
                CodigoRastreio = "LB918273645BR", NomeDestinatario = "Rafael Souza Martins", CpfDestinatario = "629.130.485-60",
                Marketplace = "Magalu", PesoKg = 12.50m, ValorDeclarado = 2349.90m, DataRecebimento = hoje.AddDays(-12),
                Status = StatusPacote.AguardandoColeta, TipoOperacao = TipoOperacao.ColetaTransportadora, Fragil = true,
                Observacoes = "Envio de vendedor de Santo André/SP. Transportadora faltou em duas janelas de coleta."
            },
            new Pacote
            {
                CodigoRastreio = "AA364729185BR", NomeDestinatario = "Juliana Oliveira Santos", CpfDestinatario = "183.456.702-53",
                Marketplace = "Shein", PesoKg = 0.65m, ValorDeclarado = 142.35m, DataRecebimento = hoje.AddDays(-4),
                DataRetirada = hoje.AddDays(-1), Status = StatusPacote.Retirado, TipoOperacao = TipoOperacao.RetiradaCliente
            },
            new Pacote
            {
                CodigoRastreio = "OD583920417BR", NomeDestinatario = "Gabriel Pereira Rocha", CpfDestinatario = "746.023.519-99",
                Marketplace = "AliExpress", PesoKg = 0.30m, ValorDeclarado = 87.40m, DataRecebimento = hoje.AddDays(-9),
                Status = StatusPacote.AguardandoRetirada, TipoOperacao = TipoOperacao.RetiradaCliente,
                Observacoes = "Importação liberada pela Receita. Cliente mora em Osasco/SP e pediu para segurar até sábado."
            },
            new Pacote
            {
                CodigoRastreio = "QM147258369BR", NomeDestinatario = "Camila Rodrigues Nunes", CpfDestinatario = "290.817.346-87",
                Marketplace = "Mercado Livre", PesoKg = 5.20m, ValorDeclarado = 459.00m, DataRecebimento = hoje.AddDays(-6),
                DataRetirada = hoje.AddDays(-5), Status = StatusPacote.Coletado, TipoOperacao = TipoOperacao.ColetaTransportadora,
                Observacoes = "Venda de loja parceira com destino a Campinas/SP."
            },
            new Pacote
            {
                CodigoRastreio = "NB692581473BR", NomeDestinatario = "Pedro Augusto Carvalho", CpfDestinatario = "853.914.620-70",
                Marketplace = "Amazon", PesoKg = 0.90m, ValorDeclarado = 249.90m, DataRecebimento = hoje.AddDays(-3),
                Status = StatusPacote.AguardandoColeta, TipoOperacao = TipoOperacao.ColetaTransportadora,
                Observacoes = "Devolução de compra em garantia, aguardando coleta para São José dos Campos/SP."
            },
            new Pacote
            {
                CodigoRastreio = "QB803716254BR", NomeDestinatario = "Larissa Mendes Barbosa", CpfDestinatario = "371.605.928-59",
                Marketplace = "Shopee", PesoKg = 2.10m, ValorDeclarado = 98.70m, DataRecebimento = hoje.AddDays(-15),
                Status = StatusPacote.Devolvido, TipoOperacao = TipoOperacao.Devolucao,
                Observacoes = "Prazo de guarda expirado. Devolvido ao remetente em Guarulhos/SP."
            },
            new Pacote
            {
                CodigoRastreio = "OY456123789BR", NomeDestinatario = "Thiago Fernandes Gomes", CpfDestinatario = "614.287.053-17",
                Marketplace = "Magalu", PesoKg = 8.75m, ValorDeclarado = 1899.00m, DataRecebimento = hoje,
                Status = StatusPacote.Recebido, TipoOperacao = TipoOperacao.RetiradaCliente, Fragil = true,
                Observacoes = "Fritadeira elétrica. Caixa com leve amassado na lateral, registrado em foto."
            },
            new Pacote
            {
                CodigoRastreio = "LB274930518BR", NomeDestinatario = "Beatriz Araújo Teixeira", CpfDestinatario = "902.473.186-04",
                Marketplace = "Shein", PesoKg = 1.05m, ValorDeclarado = 211.80m, DataRecebimento = hoje.AddDays(-8),
                DataRetirada = hoje.AddDays(-2), Status = StatusPacote.Retirado, TipoOperacao = TipoOperacao.RetiradaCliente,
                Observacoes = "Retirado pela mãe da destinatária com autorização assinada."
            },
            new Pacote
            {
                CodigoRastreio = "NL639184702BR", NomeDestinatario = "Felipe Cardoso Moreira", CpfDestinatario = "135.069.274-34",
                Marketplace = "AliExpress", PesoKg = 0.22m, ValorDeclarado = 35.90m, DataRecebimento = hoje,
                Status = StatusPacote.Recebido, TipoOperacao = TipoOperacao.RetiradaCliente
            },
            new Pacote
            {
                CodigoRastreio = "QM918364027BR", NomeDestinatario = "Isabela Martins Duarte", CpfDestinatario = "468.721.390-13",
                Marketplace = "Mercado Livre", PesoKg = 0.75m, ValorDeclarado = 129.90m, DataRecebimento = hoje.AddDays(-2),
                Status = StatusPacote.AguardandoRetirada, TipoOperacao = TipoOperacao.RetiradaCliente, Fragil = true,
                Observacoes = "Perfume em frasco de vidro."
            },
            new Pacote
            {
                CodigoRastreio = "AA507281936BR", NomeDestinatario = "Rodrigo Lima Vasconcelos", CpfDestinatario = "257.980.614-85",
                Marketplace = "Amazon", PesoKg = 4.40m, ValorDeclarado = 699.00m, DataRecebimento = hoje.AddDays(-20),
                Status = StatusPacote.Devolvido, TipoOperacao = TipoOperacao.Devolucao,
                Observacoes = "Destinatário recusou o recebimento no balcão."
            }
        ];
    }
}
