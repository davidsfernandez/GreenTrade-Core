namespace GreenTrade.Shared.Enums;

public enum CoffeeQuality
{
    StrictlySoft = 1, // Estritamente Mole
    Soft = 2,         // Mole
    Hard = 3,         // Dura
    Rio = 4,          // Rio
    RioZona = 5       // Rio Zona
}

public enum CoffeeCertification
{
    None = 0,
    RainforestAlliance = 1,
    UTZ = 2,
    FairTrade = 3,
    Organic = 4,
    CerradoMineiro = 5, // Denominación de origen
    FourC = 6 // 4C
}

public enum LotStatus
{
    Draft = 0,      // Rascunho
    Published = 1,  // Publicado no Mercado
    UnderOffer = 2, // Em Negociação
    Sold = 3        // Vendido
}
