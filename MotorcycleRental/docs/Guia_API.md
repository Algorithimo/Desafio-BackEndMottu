# 📖 Guia Completo de Cenários da API - Motorcycle Rental

## 🎯 Visão Geral

Este documento detalha todos os cenários possíveis de uso da API Motorcycle Rental, incluindo regras de negócio, validações, cálculos de planos e multas.

---

## 🏍️ Cenários de Gestão de Motos (Admin)

### **📋 Cenário 1: Cadastro de Moto**

#### **✅ Cenário de Sucesso:**
```json
POST /api/admin/motorcycles
{
  "identifier": "MOTO-2024-001",
  "year": 2024,
  "model": "Honda CG 160",
  "plate": "ABC1234"
}
```

**Resultado:**
- ✅ Moto criada no banco
- ✅ Evento publicado no RabbitMQ
- ✅ Consumer processa evento (se ano = 2024)
- ✅ Evento salvo na tabela `MotorcycleEvents`

#### **❌ Cenários de Erro:**
```json
// Placa já existe
POST /api/admin/motorcycles
{
  "plate": "ABC1234" // ❌ "Plate already exists"
}

// Ano inválido
{
  "year": 1800 // ❌ "Year must be between 1900 and 2025"
}

// Placa formato inválido
{
  "plate": "INVALID" // ❌ "Plate format is invalid"
}
```

### **📋 Cenário 2: Consulta de Motos**

```bash
# Listar todas
GET /api/admin/motorcycles

# Filtrar por placa
GET /api/admin/motorcycles?plate=ABC

# Buscar por ID
GET /api/admin/motorcycles/{id}
```

### **📋 Cenário 3: Atualização de Placa**

#### **✅ Cenário de Sucesso:**
```json
PUT /api/admin/motorcycles/{id}/plate
{
  "plate": "XYZ9876"
}
```

#### **❌ Cenários de Erro:**
```json
// Moto tem locação ativa
PUT /api/admin/motorcycles/{id}/plate
// ❌ "Cannot update plate while motorcycle has active rentals"

// Nova placa já existe
{
  "plate": "ABC1234" // ❌ "Plate already exists"
}
```

### **📋 Cenário 4: Exclusão de Moto**

#### **✅ Cenário de Sucesso:**
```bash
DELETE /api/admin/motorcycles/{id}
# ✅ Moto sem histórico de locações
```

#### **❌ Cenário de Erro:**
```bash
DELETE /api/admin/motorcycles/{id}
# ❌ "Motorcycle cannot be deleted because it has rental records"
```

---

## 👤 Cenários de Gestão de Entregadores

### **📋 Cenário 5: Cadastro de Entregador**

#### **✅ Cenário de Sucesso:**
```json
POST /api/drivers
{
  "identifier": "DRIVER-001",
  "name": "João Silva",
  "cnpj": "12345678000195",
  "birthDate": "1990-05-15",
  "cnhNumber": "12345678901",
  "cnhType": 0  // A = 0, B = 1, AB = 2
}
```

#### **❌ Cenários de Erro:**
```json
// CNPJ já existe
{
  "cnpj": "12345678000195" // ❌ "CNPJ already exists"
}

// CNH já existe
{
  "cnhNumber": "12345678901" // ❌ "CNH already exists"
}

// Menor de 18 anos
{
  "birthDate": "2010-01-01" // ❌ "Driver must be at least 18 years old"
}

// CNPJ inválido
{
  "cnpj": "123" // ❌ "CNPJ must have 14 digits"
}

// CNH inválida
{
  "cnhNumber": "123" // ❌ "CNH must have 11 digits"
}
```

### **📋 Cenário 6: Upload de Foto CNH**

#### **✅ Cenário de Sucesso:**
```bash
POST /api/drivers/{id}/cnh-image
Content-Type: multipart/form-data
File: imagem.png ou imagem.bmp
```

**Resultado:**
- ✅ Arquivo salvo em `/temp-storage/cnh/{driverId}/`
- ✅ URL atualizada no banco
- ✅ Driver.CNHImageUrl preenchido

#### **❌ Cenários de Erro:**
```bash
# Formato inválido
File: imagem.jpg // ❌ "Invalid image format. Only PNG and BMP are allowed"

# Arquivo vazio
File: (empty) // ❌ "File is required"

# Driver não existe
POST /api/drivers/invalid-id/cnh-image // ❌ "Driver not found"
```

---

## 🏍️ Cenários de Locação de Motos

### **📋 Cenário 7: Criação de Locação**

#### **✅ Cenário de Sucesso:**
```json
POST /api/rentals
{
  "driverId": "guid-do-entregador",
  "motorcycleId": "guid-da-moto",
  "plan": 0,  // Days7 = 0, Days15 = 1, Days30 = 2, Days45 = 3, Days50 = 4
  "startDate": "2024-08-15"  // Deve ser dia seguinte ou posterior
}
```

**Regras Aplicadas:**
- ✅ Data início = primeiro dia após criação
- ✅ Cálculo automático de preços por plano
- ✅ Status = Active

#### **❌ Cenários de Erro:**
```json
// Entregador não habilitado
{
  "driverId": "driver-cnh-tipo-b" // ❌ "Only drivers with CNH type A or AB are allowed"
}

// Entregador já tem locação ativa
{
  "driverId": "driver-com-locacao-ativa" // ❌ "Driver already has an active rental"
}

// Moto não disponível
{
  "motorcycleId": "moto-ja-alugada" // ❌ "Motorcycle is not available"
}

// Data de início inválida
{
  "startDate": "2024-08-13" // ❌ "Start date must be tomorrow or later"
}
```

### **📋 Cenário 8: Planos de Locação e Preços**

#### **💰 Tabela de Preços:**

| Plano | Enum | Dias | Preço/Dia | Total Exemplo |
|-------|------|------|-----------|---------------|
| Days7 | 0 | 7 dias | R$ 30,00 | R$ 210,00 |
| Days15 | 1 | 15 dias | R$ 28,00 | R$ 420,00 |
| Days30 | 2 | 30 dias | R$ 22,00 | R$ 660,00 |
| Days45 | 3 | 45 dias | R$ 20,00 | R$ 900,00 |
| Days50 | 4 | 50 dias | R$ 18,00 | R$ 900,00 |

#### **🗓️ Cálculo de Datas:**
```json
// Exemplo: Plano de 7 dias iniciando 15/08/2024
{
  "plan": 0,
  "startDate": "2024-08-15"
}

// Resultado automático:
{
  "startDate": "2024-08-15",
  "expectedEndDate": "2024-08-22", // +7 dias
  "dailyRate": 30.00,
  "totalAmount": 210.00
}
```

### **📋 Cenário 9: Simulação de Devolução**

#### **✅ Devolução no Prazo:**
```json
POST /api/rentals/{id}/simulate-return
{
  "returnDate": "2024-08-22"  // Data prevista exata
}

// Resposta:
{
  "returnDate": "2024-08-22",
  "totalDays": 7,
  "dailyRate": 30.00,
  "baseAmount": 210.00,
  "penaltyAmount": 0.00,
  "additionalAmount": 0.00,
  "finalAmount": 210.00,
  "message": "Return on expected date. No additional charges."
}
```

#### **⚠️ Devolução Antecipada (com Multa):**

##### **Plano 7 dias - Multa 20%:**
```json
POST /api/rentals/{id}/simulate-return
{
  "returnDate": "2024-08-20"  // 2 dias antes
}

// Resposta:
{
  "returnDate": "2024-08-20",
  "totalDays": 5,
  "dailyRate": 30.00,
  "baseAmount": 150.00,      // 5 dias × R$ 30
  "penaltyAmount": 12.00,    // 20% sobre 2 dias não usados (R$ 60 × 20%)
  "additionalAmount": 0.00,
  "finalAmount": 162.00,
  "message": "Early return detected. A penalty of R$ 12,00 will be applied."
}
```

##### **Plano 15 dias - Multa 40%:**
```json
// Locação de 15 dias, devolvendo 5 dias antes
POST /api/rentals/{id}/simulate-return
{
  "returnDate": "2024-08-25"  // 5 dias antes do previsto
}

// Resposta:
{
  "penaltyAmount": 56.00,    // 40% sobre 5 dias não usados (R$ 140 × 40%)
  "message": "Early return detected. A penalty of R$ 56,00 will be applied."
}
```

#### **📈 Devolução Atrasada (com Adicional):**
```json
POST /api/rentals/{id}/simulate-return
{
  "returnDate": "2024-08-25"  // 3 dias após prazo
}

// Resposta:
{
  "returnDate": "2024-08-25",
  "totalDays": 10,           // 7 dias + 3 extras
  "baseAmount": 210.00,      // Valor original do plano
  "penaltyAmount": 0.00,
  "additionalAmount": 150.00, // 3 dias × R$ 50,00
  "finalAmount": 360.00,     // R$ 210 + R$ 150
  "message": "Late return detected. An additional fee of R$ 150,00 will be applied."
}
```

### **📋 Cenário 10: Processamento de Devolução**

#### **✅ Devolução Real:**
```json
POST /api/rentals/{id}/return
{
  "returnDate": "2024-08-20"
}

// Resultado:
- Status alterado para "Completed"
- Valores calculados e salvos no banco
- Moto fica disponível para nova locação
```

#### **❌ Cenários de Erro:**
```json
// Locação não ativa
POST /api/rentals/{completed-rental-id}/return
// ❌ "Rental is not active"

// Locação não encontrada
POST /api/rentals/invalid-id/return
// ❌ "Rental not found"
```

---

## 🔍 Cenários de Consulta

### **📋 Cenário 11: Consultas de Locação**

```bash
# Buscar locação por ID
GET /api/rentals/{id}

# Listar locações de um entregador
GET /api/rentals/driver/{driverId}

# Buscar entregador por ID
GET /api/drivers/{id}

# Buscar entregador por identificador
GET /api/drivers/by-identifier/{identifier}
```

---

## 📊 Cenários de Regras de Negócio

### **📋 Cenário 12: Validações de CNH**

#### **✅ CNH Válidas para Locação:**
- **Tipo A**: ✅ Pode alugar motos
- **Tipo AB**: ✅ Pode alugar motos  
- **Tipo B**: ❌ NÃO pode alugar motos

#### **🧪 Teste de Validação:**
```json
// Driver com CNH tipo B tentando alugar
POST /api/rentals
{
  "driverId": "driver-cnh-tipo-b",
  "motorcycleId": "any-moto",
  "plan": 0
}

// ❌ Erro: "Driver cannot rent. Only drivers with CNH type A or AB are allowed"
```

### **📋 Cenário 13: Controle de Locações Simultâneas**

#### **Regra:** Um entregador só pode ter **1 locação ativa** por vez.

```json
// Entregador já tem locação ativa
POST /api/rentals
{
  "driverId": "driver-com-locacao-ativa"
}

// ❌ "Driver already has an active rental"
```

#### **Regra:** Uma moto só pode ter **1 locação ativa** por vez.

```json
// Moto já está alugada
POST /api/rentals
{
  "motorcycleId": "moto-ja-alugada"
}

// ❌ "Motorcycle is not available"
```

---

## 🚨 Cenários de Mensageria (RabbitMQ)

### **📋 Cenário 14: Eventos de Moto**

#### **✅ Moto Ano 2024:**
```json
POST /api/admin/motorcycles
{
  "year": 2024,
  "model": "Honda CG 160",
  "plate": "ABC2024"
}

// Fluxo:
1. ✅ Moto criada no banco
2. ✅ Evento publicado no RabbitMQ
3. ✅ Consumer detecta ano = 2024
4. ✅ Evento salvo na tabela MotorcycleEvents
```

#### **⚪ Moto Outro Ano:**
```json
POST /api/admin/motorcycles
{
  "year": 2023,
  "model": "Yamaha MT-03",
  "plate": "ABC2023"
}

// Fluxo:
1. ✅ Moto criada no banco
2. ✅ Evento publicado no RabbitMQ
3. ⚪ Consumer ignora (ano ≠ 2024)
4. ❌ Evento NÃO salvo na tabela
```

#### **🔍 Verificação de Eventos:**
```sql
-- Consultar eventos processados
SELECT "MotorcycleId", "Year", "EventData", "ProcessedAt" 
FROM "MotorcycleEvents" 
WHERE "Year" = 2024;
```

---

## 🔄 Cenários de Resiliência (Retry Policy)

### **📋 Cenário 15: Falha Temporária de Banco**

#### **Comportamento com Retry:**
```bash
# PostgreSQL offline
docker-compose stop postgres

# Tentativa de criar moto
POST /api/admin/motorcycles
{...}

# Logs mostram:
💾 Tentando validar e salvar moto no banco...
🔄 Retry 1/3 - Erro: Failed to connect to 127.0.0.1:5432
💾 Tentando validar e salvar moto no banco...
🔄 Retry 2/3 - Erro: Failed to connect to 127.0.0.1:5432
💾 Tentando validar e salvar moto no banco...
🔄 Retry 3/3 - Erro: Failed to connect to 127.0.0.1:5432
❌ Error creating motorcycle after all retries

# PostgreSQL online novamente
docker-compose start postgres

# Nova tentativa
POST /api/admin/motorcycles
{...}
# ✅ Sucesso imediato
```

---

## 📋 Resumo de Status Codes

| Cenário | Status Code | Resposta |
|---------|-------------|----------|
| Sucesso | 200 | `{"success": true, "data": {...}}` |
| Criação | 201 | `{"success": true, "data": {...}}` |
| Validação | 400 | `{"success": false, "errors": [...]}` |
| Não encontrado | 404 | `{"success": false, "errors": ["Not found"]}` |
| Regra de negócio | 400 | `{"success": false, "errors": ["Business rule violation"]}` |
| Erro interno | 500 | `{"success": false, "errors": ["Internal error"]}` |

---

## 🎯 Conclusão

Este guia cobre **todos os cenários possíveis** da API Motorcycle Rental, incluindo:

- ✅ **15 cenários principais** documentados
- ✅ **Regras de negócio** detalhadas  
- ✅ **Cálculos de multas** e adicionais
- ✅ **Validações** completas
- ✅ **Mensageria** com RabbitMQ
- ✅ **Resiliência** com retry policy
- ✅ **Exemplos práticos** de teste

**Para testes completos, execute os cenários na ordem sugerida!** 🚀