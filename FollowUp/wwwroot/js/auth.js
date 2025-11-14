window.bioAuthLogin = async function (basePath, account, password) {
	const url = (basePath || '/') + 'api/auth/login';
	try {
		const res = await fetch(url, {
			method: 'POST',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify({ account, password }),
			credentials: 'include'
		});
		
		const text = await res.text();
		
		return {
			success: res.ok,
			status: res.status,
			message: text || (res.ok ? '登录成功' : '登录失败')
		};
	} catch (error) {
		return {
			success: false,
			status: 0,
			message: '网络错误，请检查网络连接'
		};
	}
}



window.bioAuthLogout = async function (basePath) {
	const url = (basePath || '/') + 'api/auth/logout';
	try {
		await fetch(url, { method: 'POST', credentials: 'include' });
	} catch (e) {
		// 忽略网络错误，后续页面跳转会刷新认证状态
	}

	// 清理所有可能的认证cookie
	bioAuthClearAllCookies();

	return true;
}


window.bioAuthClearUserId = function () {
	try { localStorage.removeItem('userId'); } catch (e) { }
}

// 清理所有认证相关的cookie
window.bioAuthClearAllCookies = function () {
	try {
		// 获取当前端口
		const port = window.location.port || (window.location.protocol === 'https:' ? '443' : '80');

		// 清理按端口区分的cookie
		const cookiesToClear = [
			`access_token_${port}`,
			`refresh_token_${port}`
		];

		cookiesToClear.forEach(cookieName => {
			// 设置过期时间为过去的时间来删除cookie
			document.cookie = `${cookieName}=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/; SameSite=Lax`;
		});

		console.log('已清理认证cookie');
	} catch (e) {
		console.warn('清理cookie失败:', e);
	}
}

