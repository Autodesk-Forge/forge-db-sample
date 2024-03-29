﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[ApiController]
[Route("api/[controller]")]
public class ModelsController : ControllerBase
{
	public record BucketObject(string name, string urn);

	private readonly ForgeService _forgeService;

	public ModelsController(ForgeService forgeService)
	{
		_forgeService = forgeService;
	}

	[HttpGet()]
	public async Task<IEnumerable<BucketObject>> GetModels()
	{
		var objects = await _forgeService.GetObjects();
		return from o in objects
					 select new BucketObject(o.ObjectKey, ForgeService.Base64Encode(o.ObjectId));
	}

	[HttpGet("bucket")]
	public async Task<dynamic> GetBucketName()
	{
		string bucketKey = await _forgeService.GetBucketKey();
		dynamic bucket = new JObject();
		bucket.name = bucketKey;
		return JsonConvert.SerializeObject(bucket);
	}

	[HttpGet("{urn}/status")]
	public async Task<TranslationStatus> GetModelStatus(string urn)
	{
		try
		{
			var status = await _forgeService.GetTranslationStatus(urn);
			return status;
		}
		catch (Autodesk.Forge.Client.ApiException ex)
		{
			if (ex.ErrorCode == 404)
				return new TranslationStatus("n/a", "", new List<string>());
			else
				throw ex;
		}
	}

}