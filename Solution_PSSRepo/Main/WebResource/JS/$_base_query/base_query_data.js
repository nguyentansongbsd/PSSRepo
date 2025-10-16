/**
 * Lấy danh sách các bản ghi từ một entity dựa trên giá trị của một trường lookup.
 * Phiên bản này không sử dụng async/await.
 *
 * @param {string} entityName - Tên logic của entity bạn muốn truy vấn (ví dụ: "contacts").
 * @param {string} lookupFieldName - Tên logic của trường lookup (ví dụ: "parentcustomerid").
 * @param {string} lookupId - GUID của bản ghi trong trường lookup.
 * @returns {Promise<Array<object>>} - Một promise sẽ giải quyết với một mảng các đối tượng dữ liệu.
 */
function getEntityDataByLookup(entityName, lookupFieldName, lookupId) {
    // Trả về một đối tượng Promise mới ngay lập tức
    return new Promise((resolve, reject) => {
        // 1. Xây dựng URL cho API endpoint
        // ⚠️ Hãy nhớ thay thế bằng URL API thực tế của bạn!
        const apiEndpoint = `https://your-api-endpoint.com/api/data/v9.2/${entityName}?$filter=_${lookupFieldName}_value eq ${lookupId}`;

        const headers = {
            'OData-MaxVersion': '4.0',
            'OData-Version': '4.0',
            'Accept': 'application/json',
            'Content-Type': 'application/json; charset=utf-8',
            // Thêm các header xác thực cần thiết ở đây, ví dụ:
            // 'Authorization': 'Bearer YOUR_ACCESS_TOKEN'
        };

        // 2. Gọi fetch API, nó sẽ trả về một Promise
        fetch(apiEndpoint, { method: 'GET', headers: headers })
            .then(response => {
                // 3. Kiểm tra xem phản hồi có thành công không
                if (!response.ok) {
                    // Nếu không thành công, ném ra một lỗi để nhảy tới khối .catch()
                    throw new Error(`Lỗi HTTP! Trạng thái: ${response.status}`);
                }
                // response.json() cũng trả về một Promise, nên ta return nó
                // để chuỗi .then() tiếp theo có thể xử lý kết quả JSON
                return response.json();
            })
            .then(data => {
                // 4. Xử lý dữ liệu JSON đã được phân tích cú pháp
                // Dữ liệu thường nằm trong thuộc tính 'value' của OData
                resolve(data.value); // THÀNH CÔNG: Hoàn thành Promise với dữ liệu
            })
            .catch(error => {
                // 5. Bắt bất kỳ lỗi nào xảy ra trong chuỗi Promise
                console.error('Không thể lấy dữ liệu:', error);
                reject(error); // THẤT BẠI: Từ chối Promise với thông tin lỗi
            });
    });
}