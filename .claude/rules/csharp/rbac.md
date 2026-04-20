# RBAC — Regras Obrigatórias (C#)

- Nunca `[Authorize]` genérico em resolver de negócio — sempre `[Authorize(Policy = SystemPermissions.Xxx)]`
- Novas features protegidas: constante em `SystemPermissions` → `SystemPermissions.All` → policy no resolver
- `Login`, `RefreshToken` e `RegisterTenant` são as únicas mutations `[AllowAnonymous]`
- Permissões normalizadas no serviço: `Trim()` + `ToLowerInvariant()` + remover vazios/duplicados
- Convenção: entidades de negócio usam `read/create/update/delete`; entidades admin usam `manage`
- Seed garante que o role `Administrador` tenha todas as permissões ativas
