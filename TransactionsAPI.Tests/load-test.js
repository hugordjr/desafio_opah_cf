import http from 'k6/http';
import { sleep, check } from 'k6';

export let options = {
    stages: [
        { duration: '10s', target: 10 }, // Subir para 10 usuários
        { duration: '30s', target: 50 }, // Manter 50 usuários
        { duration: '10s', target: 0 },  // Reduzir para 0 usuários
    ],
};

export default function () {
    const url = 'http://localhost:5041/api/transactions'; // Verifique se está na porta correta
    const payload = JSON.stringify({
        description: 'Test Transaction',
        amount: 100,
        date: new Date().toISOString(),
        type: 'Credit',
    });

    const params = {
        headers: {
            'Content-Type': 'application/json',
            Authorization: 'Bearer <seu_token_jwt>', // Substitua pelo token JWT correto
        },
    };

    const res = http.post(url, payload, params);

    console.log(`Response: ${res.body}`); // Adicione log para verificar a resposta

    check(res, {
        'status é 202': (r) => r.status === 202,
        'resposta contém TraceId': (r) => {
            const body = JSON.parse(r.body || '{}');
            return body.TraceId !== undefined;
        },
    });

    sleep(1); // Simular intervalo de 1 segundo entre requisições
}
