#region Copyright
/*************************************************************
//   Vulkan C# class 
//   Filename CVulkan.cs
//   created : Xingxiang JIAN, jiansir@163.com
//   last updated : 3 Nov 2021
/*************************************************************
 *   Bindings Installed 
 *   GLMNet by Dave Kerr
 *   Glfw.NET by realvictorprm
 *   SharpVulkan by techlabxe
 *   System.Numerics.Vectors
 *   using Vulkan;
 useage steps:
 *
 *    1.initialize vulkan and binding the current window
 *      InitVulkan(windowHandle);
 *    2.send drawing commands
 *      //Initialize matrix
        LoadIdentity();
        LookAt(eye, center, up);
        PushMatrix();
          Translate(x, y, z);
          Rotate(angle,0,1,0);
          Scale(1,2,1);
          SetColor(color)
          ... //draw objects
        PopMatrix();
      3.how to draw primitives? use begin() and end()
        (1) binding vertices array and indices array
        Begin(DrawingPrimitive type); //type default is TRIANGLE_LIST
          VertexArray(Vertex3D[] vertices);
          VertexIndexArray(int[] indices);
        End();
        (2) transform point one by one
         Begin(); //default is TRIANGLE_LIST
           Vertex(Vertex3D point);
           VertexIndex(int index);
         End();
      4.using texture
        call LoadTexture(string texFile) before drawing;      
      5.window OnSize
        onWindowResized(newWidth, newHeight);
      6.use other shaders: shader must be compiled as .spv files.
       call LoadShader(string vertSpvFile, string fragSpvFile) before drawing;
       default shader is defaultvert.spv and defaultfrag.spv
      7.UpdateDraw()
        call UpdateDraw() while update drawing, don't try to send command again
      8.Asynchronous Drawing
        each Begin()..End() is independent , fell free to send it to drawing buffer
        asynchronously.

-----------debug reports:---------------------------
 sharpvulkan can't support DeviceFeatures, render will failed 
 while physical device's featureNotSolidFillMode = false

 pick device 2, objects transform will failed,all are same????
 
 ** modification histories *************************
 *  2018.11.22 by jian : Add function CreateDefaultGraphicsPipeLine()
 *  this function create all graphicPipeline and add to array pipeLines
 *  that covers all primitives and do not need to create it dynamic
 *  3 Nov 2021 by jiansir, 
 *  to Create a multi-thread version for CreateTexture to improve speed 
 ********************************/
#endregion Copyright
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using GlmNet;
using SharpVulkan;
using glfw3;

//should be replaced meet plate-independent
using System.Drawing;
using System.Drawing.Imaging;
using DataCollection;

namespace Graphics3D
{
    public struct VkPhysicalDeviceFeatures1
    {
        public VkBool32 robustBufferAccess;
        public VkBool32 fullDrawIndexUint32;
        public VkBool32 imageCubeArray;
        public VkBool32 independentBlend;
        public VkBool32 geometryShader;
        public VkBool32 tessellationShader;
        public VkBool32 sampleRateShading;
        public VkBool32 dualSrcBlend;
        public VkBool32 logicOp;
        public VkBool32 multiDrawIndirect;
        public VkBool32 drawIndirectFirstInstance;
        public VkBool32 depthClamp;
        public VkBool32 depthBiasClamp;
        public VkBool32 fillModeNonSolid;
        public VkBool32 depthBounds;
        public VkBool32 wideLines;
        public VkBool32 largePoints;
        public VkBool32 alphaToOne;
        public VkBool32 multiViewport;
        public VkBool32 samplerAnisotropy;
        public VkBool32 textureCompressionETC2;
        public VkBool32 textureCompressionASTC_LDR;
        public VkBool32 textureCompressionBC;
        public VkBool32 occlusionQueryPrecise;
        public VkBool32 pipelineStatisticsQuery;
        public VkBool32 vertexPipelineStoresAndAtomics;
        public VkBool32 fragmentStoresAndAtomics;
        public VkBool32 shaderTessellationAndGeometryPointSize;
        public VkBool32 shaderImageGatherExtended;
        public VkBool32 shaderStorageImageExtendedFormats;
        public VkBool32 shaderStorageImageMultisample;
        public VkBool32 shaderStorageImageReadWithoutFormat;
        public VkBool32 shaderStorageImageWriteWithoutFormat;
        public VkBool32 shaderUniformBufferArrayDynamicIndexing;
        public VkBool32 shaderSampledImageArrayDynamicIndexing;
        public VkBool32 shaderStorageBufferArrayDynamicIndexing;
        public VkBool32 shaderStorageImageArrayDynamicIndexing;
        public VkBool32 shaderClipDistance;
        public VkBool32 shaderCullDistance;
        public VkBool32 shaderFloat64;
        public VkBool32 shaderInt64;
        public VkBool32 shaderInt16;
        public VkBool32 shaderResourceResidency;
        public VkBool32 shaderResourceMinLod;
        public VkBool32 sparseBinding;
        public VkBool32 sparseResidencyBuffer;
        public VkBool32 sparseResidencyImage2D;
        public VkBool32 sparseResidencyImage3D;
        public VkBool32 sparseResidency2Samples;
        public VkBool32 sparseResidency4Samples;
        public VkBool32 sparseResidency8Samples;
        public VkBool32 sparseResidency16Samples;
        public VkBool32 sparseResidencyAliased;
        public VkBool32 variableMultisampleRate;
        public VkBool32 inheritedQueries;
    }

    //model description
    public class CModelDescriptor : CModel
    {
        public VkDevice device = null;
        public VkGraphicsPipelineCreateInfo pipelineInfo = null;
        public VkPipeline graphicsPipeline = null;
        public VkDescriptorSet descriptorSet = null;
        public VkPipelineLayout pipelineLayout = null;
        public VkDescriptorSetLayout descriptorSetLayout = null;

        public VkBuffer vertexBuffer = null;
        public VkBuffer indexBuffer = null;
        public VkBuffer uniformBuffer = null;        
        public VkDeviceMemory vertexBufferMemory = null;
        public VkDeviceMemory indexBufferMemory = null;
        public VkDeviceMemory uniformBufferMemory = null;

        public VkImage textureImage = null;
        public Bitmap textureBitmap = null;

        public VkDeviceMemory textureImageMemory = null;
        public VkImageView textureImageView = null;
        public VkSampler textureSampler = null;

        public uint indicesLength = 0;
        
        private object modelLocker = new object();        
        public CModelDescriptor()
        {
            CreateHashKey();
            device = null;
            pipelineInfo = null;
            graphicsPipeline = null;
            pipelineLayout = null;

            descriptorSet = null;
            bInitialized = false;
            vertexBuffer = null;
            indexBuffer = null;
            uniformBuffer = null;
            uniformBufferMemory = null;
            indexBufferMemory = null;
            vertexBufferMemory = null;
            textureImage = null;
            textureBitmap = null;
            textureImageMemory = null;
            textureImageView = null;
            textureSampler = null;

            m_modelMatrix = new UniformBufferObject(true);
            m_translate = new vec3(0, 0, 0);
            m_scale = new vec3(1, 1, 1);
            m_rotate = new vec3(0, 0, 0);//x,y,z axe angle

            indicesLength = 0;
            errMessage = "";
        }
        //reserved
        public bool Initialize()
        {
            bInitialized = true;
            return true;
        }
        // release model's resources
        public override void Release()
        {
            try
            {
                CVulkan.ReleaseBuffer(device,ref uniformBuffer);
                CVulkan.ReleaseBufferMemory(device, ref uniformBufferMemory);
                CVulkan.ReleaseBuffer(device, ref indexBuffer);
                CVulkan.ReleaseBufferMemory(device, ref indexBufferMemory);
                CVulkan.ReleaseBuffer(device, ref vertexBuffer);
                CVulkan.ReleaseBufferMemory(device, ref vertexBufferMemory);
                //CVulkan.ReleaseBufferMemory(device, ref textureImageMemory);
                //CVulkan.ReleaseImage(device, ref textureImage);
                //CVulkan.ReleaseImageView(device, ref textureImageView);
                //CVulkan.ReleaseSampler(device, ref textureSampler);
                if (textureBitmap != null)
                {
                    textureBitmap.Dispose();
                    textureBitmap = null;
                }                          

                m_modelMatrix.Clear();

                //VulkanAPI.vkDestroyPipeline();
                device = null;
                pipelineInfo = null;
                graphicsPipeline = null;
                pipelineLayout = null;
                descriptorSet = null;
                bInitialized = false;

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public void ReleaseTextureContent()
        {
            CVulkan.ReleaseSampler(device, ref textureSampler);
            CVulkan.ReleaseImage(device, ref textureImage);
            CVulkan.ReleaseBufferMemory(device, ref textureImageMemory);            
            CVulkan.ReleaseImageView(device, ref textureImageView);
        }
        //deal with model matrix,translate,rotate,scale
        public override bool UpdateUniformBuffer()
        {
            if (uniformBufferMemory == null) return false;
            UniformBufferObject ubo = m_modelMatrix.Copy();

            //if (m_translate.x != 0.0 || m_translate.y != 0.0 || m_translate.z != 0.0)
            // if (m_enableTranslate)
            ubo.model = glm.translate(ubo.model, m_translate);
            // if(m_enableScale)
            {
                ubo.model = glm.scale(ubo.model, m_scale);
            }
            //clockwise 
            // if (m_enableRotate)
            {
                ubo.model = glm.rotate(ubo.model, glm.radians(m_rotate.x), new vec3(1, 0, 0));
            }
            // if (m_enableRotate)
            {
                ubo.model = glm.rotate(ubo.model, glm.radians(m_rotate.y), new vec3(0, 1, 0));
            }
            // if (m_enableRotate)
            {
                ubo.model = glm.rotate(ubo.model, glm.radians(m_rotate.z), new vec3(0, 0, 1));
            }

            //map memory            
            int bufferSize = UniformBufferObject.GetSize();
            MappedMemoryStream mappedStream;
            if (VulkanAPI.vkMapMemory(device, uniformBufferMemory, 0,
                                       bufferSize, 0, out mappedStream)
                != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "failed to map memory!";
                return false;
            }
            // write ubo data to mapped stream
            mappedStream.Write(ubo.toFloatArray());
            VulkanAPI.vkUnmapMemory(device, uniformBufferMemory);
            return true;
        }
        // update model's resource description
        public void UpdateDescriptorSet()
        {
            if (descriptorSet == null) return;
            if (uniformBuffer == null) return;
            VkDescriptorBufferInfo bufferInfo = new VkDescriptorBufferInfo();
            bufferInfo.buffer = uniformBuffer;
            bufferInfo.offset = 0;
            bufferInfo.range = UniformBufferObject.GetSize();

            VkWriteDescriptorSet[] descriptorWrites;
            if (textureImageView != null && textureSampler != null)
                descriptorWrites = new VkWriteDescriptorSet[2];
            else descriptorWrites = new VkWriteDescriptorSet[1];

            descriptorWrites[0] = new VkWriteDescriptorSet();
            descriptorWrites[0].dstSet = descriptorSet;
            descriptorWrites[0].dstBinding = 0;
            descriptorWrites[0].dstArrayElement = 0;
            descriptorWrites[0].descriptorType = VkDescriptorType.VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
            descriptorWrites[0].descriptorCount = 1;
            descriptorWrites[0].pBufferInfo = new[] { bufferInfo };

            if (textureImageView != null && textureSampler != null)
            {
                VkDescriptorImageInfo imageInfo = new VkDescriptorImageInfo();
                imageInfo.imageLayout = VkImageLayout.VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
                imageInfo.imageView = textureImageView;
                imageInfo.sampler = textureSampler;

                descriptorWrites[1] = new VkWriteDescriptorSet();
                descriptorWrites[1].dstSet = descriptorSet;
                descriptorWrites[1].dstBinding = 1;
                descriptorWrites[1].dstArrayElement = 0;
                descriptorWrites[1].descriptorType = VkDescriptorType.VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER;
                descriptorWrites[1].descriptorCount = 1;
                descriptorWrites[1].pImageInfo = new[] { imageInfo };
            }
            VulkanAPI.vkUpdateDescriptorSets(device, descriptorWrites, null);
        }
    }


    public class QueueFamilyIndices
    {
        public int graphicsFamily = -1;
        public int presentFamily = -1;
        public bool isComplete()
        {
            return graphicsFamily >= 0 && presentFamily >= 0;
        }
    };
    struct SwapChainSupportDetails
    {
        public VkSurfaceCapabilitiesKHR capabilities;
        public VkSurfaceFormatKHR[] formats;
        public VkPresentModeKHR[] presentModes;
    };

    public class CVulkan : CGraphic3D
    {
#if DEBUG
        const bool enableValidationLayers = false;
#else
    const bool enableValidationLayers = false;
#endif
        //error messages and validation layer reports
        private List<string> validationLayerReports = new List<string>();
        private void AddToReports(string report)
        {
            validationLayerReports.Add(report);
        }
        public string GetValidationLayerReports()
        {
            string info = "";
            for (int i = 0; i < validationLayerReports.Count; i++)
            {
                info += "\n";
                info += validationLayerReports[i];
            }
            return info;
        }
        public void ClearValidationLayerReports() { validationLayerReports.Clear(); }

        private string[] validationLayers = null;
        private VkLayerProperties[] availableLayers = null;
        private string[] availableLayersArray = null;
        private string[] requiredExtensions = null;
        private string[] deviceExtensions = null;
        private VkInstance vkInstance = null;
        private VkSurfaceKHR vulkanSurface = null;

        private VkPhysicalDevice[] physicalDevices = null;  //pysical devices list
        private VkPhysicalDevice physicalDevice = null;     //selected physical device
        private VkDevice device = null;               //logical device

        private VkSwapchainKHR swapChain = null;
        private VkImage[] swapChainImages = null;
        private VkImageView[] swapChainImageViews = null;
        private VkFormat swapChainImageFormat;
        private VkExtent2D swapChainExtent;

        private VkImage textureImage = null;
        //public Bitmap textureBitmap = null;
        private VkDeviceMemory textureImageMemory = null;
        private VkImageView textureImageView = null;
        private VkSampler textureSampler = null;

        public VkFormat textureBitmapFormat = VkFormat.VK_FORMAT_R8G8B8_UNORM;
        public VkSamplerCreateInfo textureSamplerInfo = null;

        private VkQueue graphicsQueue = null;
        private VkQueue presentQueue = null;
        private VkRenderPass renderPass = null;

        private VkFramebuffer[] swapChainFramebuffers = null;

        public VkBuffer vertexBuffer = null;
        public VkDeviceMemory vertexBufferMemory = null;
        public VkBuffer indexBuffer = null;
        public VkDeviceMemory indexBufferMemory = null;
        private VkBuffer uniformBuffer = null;
        private VkDeviceMemory uniformBufferMemory = null;

        private VkImage depthImage = null;
        private VkDeviceMemory depthImageMemory = null;
        private VkImageView depthImageView = null;

        private VkCommandPool commandPool;
        private VkCommandBuffer[] commandBuffers = null;
        private VkSemaphore imageAvailableSemaphore = null;
        private VkSemaphore renderFinishedSemaphore = null;

        private VkDescriptorSetLayout descriptorSetLayout = null;
        private VkDescriptorPool descriptorPool = null;
        private VkDescriptorSet descriptorSet = null;

        public GLFWwindow glfwWindow;       // = glfwCreateWindow()
        private IntPtr windowHandle = IntPtr.Zero;  // = WinForm handle
        private Semaphore m_Semaphore = null;
        //this function not avialable now
#pragma warning disable CS0169 // 从不使用字段“CVulkan.devceLimits”
        private VkPhysicalDeviceLimits devceLimits;
#pragma warning restore CS0169 // 从不使用字段“CVulkan.devceLimits”

        //pipelinelayout info 
        private VkPipelineInputAssemblyStateCreateInfo inputAssembly = null;
        private VkPipelineViewportStateCreateInfo viewportState = null;
        public VkPipelineRasterizationStateCreateInfo rasterizer = null;
        private VkPipelineMultisampleStateCreateInfo multiSampling = null;
        private VkPipelineColorBlendStateCreateInfo colorBlending = null;

        private VkGraphicsPipelineCreateInfo pipelineInfo = null;
        private List<VkGraphicsPipelineCreateInfo> pipelineInfos = new List<VkGraphicsPipelineCreateInfo>();
        private List<VkGraphicsPipelineCreateInfo> pipelineInfoMatrix = new List<VkGraphicsPipelineCreateInfo>();

        private VkPipelineLayout pipelineLayout = null;
        private VkPipeline graphicsPipeline = null;
        private List<VkPipeline> graphicsPipelines = new List<VkPipeline>();

        private VkShaderModule vertShaderModule = null;
        private VkShaderModule fragShaderModule = null;

        //private CModelDescriptor modelDescriptor;
        //private List<CModelDescriptor> pModelDescriptores = new List<CModelDescriptor>();        

        private VkPhysicalDeviceFeatures1 deviceFeatures = new VkPhysicalDeviceFeatures1();
        public VkPhysicalDeviceFeatures1 GetDeviceFeatures(VkPhysicalDevice device)
        {
            VkPhysicalDeviceFeatures feature;
            VulkanAPI.vkGetPhysicalDeviceFeatures(device, out feature);
            //transform feature to deviceFeatures
            // deviceFeatures = feature;
            return deviceFeatures;
        }

        #region Release Memory Functions
        static public void ReleaseBuffer(VkDevice _device, ref VkBuffer buffer)
        {
            if (buffer != null)
            {
                VulkanAPI.vkDestroyBuffer(_device, buffer);
                buffer = null;
            }
        }
        static public void ReleaseBufferMemory(VkDevice _device, ref VkDeviceMemory memory)
        {
            if (memory != null)
            {
                VulkanAPI.vkFreeMemory(_device, memory);
                memory = null;
            }
        }
        static public void ReleaseImage(VkDevice _device, ref VkImage image)
        {
            if (image != null)
            {
                //VulkanAPI.vkDestroyImage(_device, image);
                image = null;
            }
        }

        static public void ReleaseImageView(VkDevice _device, ref VkImageView imageview)
        {
            if (imageview != null)
            {
                //VulkanAPI.vkDestroyImageView(_device, imageview);
                imageview = null;
            }
        }

        static public void ReleaseSampler(VkDevice _device, ref VkSampler sampler)
        {
            if (sampler != null)
            {                
                //VulkanAPI.vkDestroySampler(_device, sampler);
                sampler = null;
            }
        }
        /// <summary>
        /// 透明检测，检测前10个样本点
        /// </summary>
        /// <returns>是否透明</returns>
        public override bool IsTransparentDraw()
        {
            if( verticesArray == null )
            {
                for( int i = 0; i < 10 && i < pVertics.Count; i++ )
                {
                    if ( pVertics[i].color.w < 1.0f ) 
                        return true;
                }
            }
            else             
            {
                for( int i = 0; i < 10 && i < verticesArray.Length; i++ )
                {
                    if (verticesArray[i].color.w < 1.0f) 
                        return true;
                }
            }
            return false;
        }

        public override void Begin(DrawingPrimitive _primitive = DrawingPrimitive.TRIANGLE_LIST)
        {
            currentModel = new CModelDescriptor();
            // modelDescriptor.m_modelMatrix = m_modelMatrix;
            SetPrimitive(_primitive);
            pVertics.Clear();
            pIndices.Clear();
        }
        #endregion Release Memory Functions

        public override bool End(bool _createVerticesBuffer = true,
                         bool _createIndexBuffer = true,
                         bool _createUniformBuffer = true)
        {
            if (_createVerticesBuffer)
            {
                if (verticesArray == null)
                {
                    //bug fixed by jian 2019.8.26, avoid to create invalid model object
                    if (pVertics.Count == 0) return false;
                    verticesArray = pVertics.ToArray();
                    pVertics.Clear();
                }
                if (!createVertexBuffer(verticesArray,out vertexBuffer,out vertexBufferMemory))
                {
                    ClearCurrentBuffer();
                    return false;
                }                
            }

            CModelDescriptor model = currentModel as CModelDescriptor;
            model.vertexBuffer = vertexBuffer;
            model.Transparent = IsTransparentDraw();

            if (_createIndexBuffer)
            {
                if (indicesArray == null)
                {
                    //bug fixed by jian 2019.8.26, avoid to create invalid model object
                    if (pIndices.Count == 0) return false;
                    indicesArray = pIndices.ToArray();
                    pIndices.Clear();
                }
                if (!createIndexBuffer(indicesArray,out indexBuffer,out indexBufferMemory))
                {
                    ClearCurrentBuffer();
                    return false;
                }
            }
            model.indexBuffer = indexBuffer;
            model.indicesLength = (uint)indicesArray.Length;
            if ( bEnableTexture )
            {
                model.textureImageMemory = textureImageMemory;
                model.textureImage = textureImage;
                model.textureImageView = textureImageView;
                model.textureSampler = textureSampler;
            }
            else
            {
                model.textureImageMemory = null;
                model.textureImage = null;
                model.textureImageView = null;
                model.textureSampler = null;
            }
            //create uniform buffer
            if (_createUniformBuffer)
                if (!createUniformBuffer())
                {
                    ClearCurrentBuffer();
                    return false;
                }
            model.uniformBuffer = uniformBuffer;
            model.uniformBufferMemory = uniformBufferMemory;

            model.device = device;
            model.descriptorSet = descriptorSet;

            //search from list to find there same one exist
            if (!GetExistGraphicPipeline(pipelineInfo)) //failed           
            {
                if (!CreateGraphicsPipeLine(pipelineInfo))     //failed
                {
                    ClearCurrentBuffer();
                    return false;
                }
            }
            model.graphicsPipeline = graphicsPipeline;
            model.descriptorSetLayout = descriptorSetLayout;
            model.pipelineLayout = pipelineLayout;
            model.pipelineInfo = CopyPipelineInfo(pipelineInfo);
            model.pipelineInfo.renderPass = renderPass;

            //model matrix
            model.m_modelMatrix = m_modelMatrix.Copy();
            model.m_translate = m_translate;
            model.m_scale = m_scale;
            model.m_rotate = m_rotate;

            model.m_enableTranslate = enableTranslate;
            model.m_enableScale = enableScale;
            model.m_enableRotate = enableRotate;

            AddModel(model);

            pVertics.Clear();
            pIndices.Clear();
            if (_createVerticesBuffer) verticesArray = null;
            if (_createIndexBuffer) indicesArray = null;
            if (_createUniformBuffer)
            {
                uniformBuffer = null;
                uniformBufferMemory = null;
            }

            //textureSampler = null;

            return true;
        }

        private void ClearCurrentBuffer()
        {
            pVertics.Clear();
            pIndices.Clear();

            if (currentModel != null)
            { 
                currentModel.Release();
                currentModel = null;
            }
            CVulkan.ReleaseBuffer(device, ref uniformBuffer);
            CVulkan.ReleaseBufferMemory(device, ref uniformBufferMemory);
            CVulkan.ReleaseBuffer(device, ref indexBuffer);
            CVulkan.ReleaseBufferMemory(device, ref indexBufferMemory);
            CVulkan.ReleaseBuffer(device, ref vertexBuffer);
            CVulkan.ReleaseBufferMemory(device, ref vertexBufferMemory);

            //CVulkan.ReleaseMemory(device, textureImageMemory);
            //CVulkan.ReleaseImage(device, textureImage);
            //CVulkan.ReleaseImageView(device, textureImageView);
            //CVulkan.ReleaseSampler(device, textureSampler);           
        }
        
        public override bool UpdateModelVertexBuffer(CModel model, Object obj)
        {
            Vertex3D[] points = obj as Vertex3D[];
            VkBuffer buffer;
            VkDeviceMemory memory;
            if( createVertexBuffer(points,out buffer,out memory) )
            {
                CVulkan.ReleaseBuffer(device, ref vertexBuffer);
                CVulkan.ReleaseBufferMemory(device, ref vertexBufferMemory);
                CModelDescriptor m = model as CModelDescriptor;
                m.vertexBuffer = buffer;
                m.vertexBufferMemory = memory;
                return true;
            }
            return false;
        }
        public override bool UpdateModelIndexBuffer(CModel model, Object obj)
        {
            int[] indices = obj as int[];
            VkBuffer buffer;
            VkDeviceMemory memory;
            if (createIndexBuffer(indices, out buffer, out memory))
            {
                CVulkan.ReleaseBuffer(device, ref indexBuffer);
                CVulkan.ReleaseBufferMemory(device, ref indexBufferMemory);
                CModelDescriptor m = model as CModelDescriptor;
                m.indexBuffer = buffer;
                m.indexBufferMemory = memory;
                return true;
            }
            return false;
        }
        
        public override bool UpdateModelTextureImage(CModel model, object obj)
        {
            string filename = obj as string;
            Bitmap bmp = LoadTexture(filename);
            if (bmp == null) return false;
            textureBitmap = bmp;
            if (!CreateTextureImageContext()) return false;
            CModelDescriptor md = model as CModelDescriptor;
            md.ReleaseTextureContent();
            md.textureImage = textureImage;
            md.textureImageMemory = textureImageMemory;
            md.textureBitmap = textureBitmap;
            md.textureImageView = textureImageView;
            md.textureSampler = textureSampler;
            return true;            
        }

        public override void EnableTexture(bool enabled = true)
        {
            bEnableTexture = enabled;
        }
        public override void DisableTexture(bool enabled = false)
        {
            textureBitmap = null;
            bEnableTexture = enabled;
        }
        private bool CreateTextureImageContext()
        {
            if (textureBitmap != null)//texture enabled
            {
                if (!createTextureImage(out textureImage,out textureImageMemory)) return false;
                if (!createTextureImageView()) return false;
                if (!createTextureSampler()) return false;
                return true;
            }
            return true;
        }
        public override void VertexArray(Vertex3D[] vertices)
        {
            verticesArray = null;
            if (vertices.Length < 1) return;
            verticesArray = new Vertex3D[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
                verticesArray[i] = vertices[i];
        }
        public override void VertexIndexArray(int[] indices)
        {
            indicesArray = null;
            if (indices.Length < 1) return;
            indicesArray = new int[indices.Length];
            for (int i = 0; i < indices.Length; i++)
                indicesArray[i] = indices[i];
        }
        //translate current model
        public override void Translate(float xoff, float yoff, float zoff)
        {
            Translate(new vec3(xoff, yoff, zoff));
        }
        //translate current model
        public override void Translate(vec3 _translate)
        {
            //m_modelMatrix.model = glm.translate(m_modelMatrix.model,_translate);
            m_translate.x += _translate.x;

            //convert vulkan y axe to downwards
            m_translate.y -= _translate.y;

            m_translate.z += _translate.z;
        }
        public VkVertexInputBindingDescription[] getBindingDescription()
        {
            VkVertexInputBindingDescription bindingDescription = new VkVertexInputBindingDescription();
            bindingDescription.binding = 0;
            bindingDescription.inputRate = VkVertexInputRate.VK_VERTEX_INPUT_RATE_VERTEX;
            bindingDescription.stride = (uint)Vertex3D.GetSize();
            return new VkVertexInputBindingDescription[] { bindingDescription };
        }
        public VkVertexInputAttributeDescription[] getAttributeDescriptions()
        {
            VkVertexInputAttributeDescription[] descriptions = new VkVertexInputAttributeDescription[4];
            uint offset = 0;
            //vec3 pos;
            descriptions[0].binding = 0;
            descriptions[0].location = 0;   //location 0
            descriptions[0].offset = offset;
            descriptions[0].format = VkFormat.VK_FORMAT_R32G32B32_SFLOAT;
            offset += 3 * sizeof(float);
            //vec4 color; 
            descriptions[1].binding = 0;
            descriptions[1].location = 1;   //location 1
            descriptions[1].offset = offset;
            descriptions[1].format = VkFormat.VK_FORMAT_R32G32B32A32_SFLOAT;
            offset += 4 * sizeof(float);
            //vec2 texCoord;
            descriptions[2].binding = 0;
            descriptions[2].location = 2;   //location 2
            descriptions[2].offset = offset;
            descriptions[2].format = VkFormat.VK_FORMAT_R32G32_SFLOAT;
            offset += 2 * sizeof(float);
            //vec3 normal
            descriptions[3].binding = 0;
            descriptions[3].location = 3;   //location 3
            descriptions[3].offset = offset;
            descriptions[3].format = VkFormat.VK_FORMAT_R32G32B32_SFLOAT;
            return descriptions;
        }
        /*
        // filter the duplicated vertics and re-caculate the color
        // and normal
        public void ResortVertics()
        {
            int n = pVertics.Count;
            if (n < 2) return;
           
            int[] orgindexes = new int[n];
            for (int i = 0; i < n; i++) orgindexes[i] = i;
            double dist = 0;
            double x1, y1, z1, x2, y2, z2;
            double minx=0, maxx=0, miny=0, maxy=0, minz=0, maxz=0;            
            for (int i = 0; i < n; i++)
            {
                x1 = pVertics[i].pos.x;
                y1 = pVertics[i].pos.y;
                z1 = pVertics[i].pos.z;
                if (i == 0)
                {
                    minx = maxx = x1;
                    miny = maxy = y1;
                    minz = maxz = z1;
                }
                else
                {
                    if (x1 < minx ) minx = x1;
                    if (y1 < miny ) miny = y1;
                    if (z1 < minz ) minz = z1;
                    if (x1 > maxx) maxx = x1;
                    if (y1 > maxy) maxy = y1;
                    if (z1 > maxz) maxz = z1;
                }
            }
            double zerox = (maxx - minx) / 10000;
            double zeroy = (maxy - miny) / 10000;
            double zeroz = (maxz - minz) / 10000;
            double zerodist = zerox * zerox + zeroy * zeroy + zeroz * zeroz;
            List<int> sames = new List<int>();
            List<int> to_removed = new List<int>();
            float r, g, b,a,x,y,z;
            for (int i = 0; i < pVertics.Count; i++)
            {
                // have been done, same to the previous 
                if (orgindexes[i] != i) continue;

                for (int j = i + 1; j < pVertics.Count; j++)
                {
                    x1 = pVertics[i].pos.x; y1 = pVertics[i].pos.y; z1 = pVertics[i].pos.z;
                    x2 = pVertics[j].pos.x; y2 = pVertics[j].pos.y; z2 = pVertics[j].pos.z;
                   // dist = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2) + (z1 - z2) * (z1 - z2);
                  //  if (dist <= zerodist) //same point
                   //     sames.Add(j);
                }              
                if( sames.Count>0)
                {
                    r = pVertics[i].color.x;
                    g = pVertics[i].color.y;
                    b = pVertics[i].color.z;
                    a = pVertics[i].color.w;                    
                    x = pVertics[i].normal.x;
                    y = pVertics[i].normal.y;
                    z = pVertics[i].normal.z;                    
                    for (int k=0;k<sames.Count;k++)
                    {
                        r += pVertics[sames[k]].color.x;
                        g += pVertics[sames[k]].color.y;
                        b += pVertics[sames[k]].color.z;
                        a += pVertics[sames[k]].color.w;
                        x += pVertics[sames[k]].normal.x;
                        y += pVertics[sames[k]].normal.y;
                        z += pVertics[sames[k]].normal.z;
                    }
                    r = r / (sames.Count+1);
                    g = g / (sames.Count + 1);
                    b = b / (sames.Count + 1);
                    a = a / sames.Count;
                    x = x / (sames.Count + 1);
                    y = y / (sames.Count + 1);
                    z = z / (sames.Count + 1);
                    pVertics[i].SetColor(r,g,b,a);
                    pVertics[i].SetNormal(x,y,z);

                    //add to be removed                    
                    for (int k =0;k< sames.Count;k++)
                    {
                        to_removed.Add(sames[k]);
                        orgindexes[sames[k]] = i; //modify the index
                    }
                    sames.Clear();
                }
            }
            // sort to_removed on index 
            for (int i = 0; i < to_removed.Count; i++)
                for (int j = i+1; j < to_removed.Count; j++)
            {
                if( to_removed[i] > to_removed[j] )
                {
                   int id = to_removed[i];
                   to_removed[i] = to_removed[j];
                   to_removed[j] = id;
                }
            }
            //remove from list
            for (int k = to_removed.Count - 1; k >= 0; k--)
            {
                pVertics.RemoveAt(to_removed[k]);
                for(int i= 0;i<n;i++)
                {
                    if (orgindexes[i] >= to_removed[k])
                        orgindexes[i]--;
                }
            }
            to_removed.Clear();
            //modify the indices array
            for (int i = 0; i < n; i++)
            {
                pIndices[i] = orgindexes[i];
            }   
        }
        */


        public override void DrawString(string text, Font font, Color color, float size,
                                        Vector64 start, Vector64 direct,Vector64 up, 
                                        TextHorizontalAlignment horAlignment = TextHorizontalAlignment.Left, 
                                        TextVerticalAlignment verAlignment = TextVerticalAlignment.Center)
        {
            PushMatrix();            

            byte r = (byte)(color.R);
            byte g = (byte)(color.G);
            byte b = (byte)(color.B);
            Color backcolor = Color.FromArgb(0,r, g, b);
            BitmapString bm = new BitmapString(text, font, color, backcolor);

            EnableTexture(true);
            BindTexture(bm.Draw());
            
            double width = text.Length * size * 0.1;
            double height = width * textureBitmap.Height / (double)textureBitmap.Width;
            //      |
            //      p1-----p2
            //      |      |
            //      p0-----p3-->
            Vector64 p0, p1, p2, p3;
            if (verAlignment == TextVerticalAlignment.Top)
            {
                //      |
                //      p1-----p2-->
                //      |      |
                //      p0-----p3
                p1 = start;
                p2 = p1 + width*direct;
                p0 = p1 - up*height;
                p3 = p2 - up*height; 
            }
            else if (verAlignment == TextVerticalAlignment.Bottom)
            {
                //      |
                //      p1-----p2
                //      |      |
                //      p0-----p3-->
                p0 = start;
                p3 = p0 + width * direct;
                p1 = p0 + up * height;
                p2 = p3 + up * height;
            }
            else //if (verAlignment == TextVerticalAlignment.Center)
            {
                //      |
                //      p1-----p2
                //      |      |-->
                //      p0-----p3
                p0 = start - 0.5 * height * up;
                p3 = p0 + width * direct;
                p1 = p0 + up * height;
                p2 = p3 + up * height;
            }

            if (horAlignment == TextHorizontalAlignment.Center)
            {
                p0 = p0 - width * 0.5 * direct;
                p1 = p1 - width * 0.5 * direct;
                p2 = p2 - width * 0.5 * direct;
                p3 = p3 - width * 0.5 * direct;
            }
            else if (horAlignment == TextHorizontalAlignment.Right)
            {
                p0 = p0 - width * 1.0 * direct;
                p1 = p1 - width * 1.0 * direct;
                p2 = p2 - width * 1.0 * direct;
                p3 = p3 - width * 1.0 * direct;
            }
            else //if (horAlignment == TextHorizontalAlignment.Left)
            {

            }
            Vertex3D v0 = new Vertex3D((float)p0.x, (float)p0.y, (float)p0.z);
            Vertex3D v1 = new Vertex3D((float)p1.x, (float)p1.y, (float)p1.z);
            Vertex3D v2 = new Vertex3D((float)p2.x, (float)p2.y, (float)p2.z);
            Vertex3D v3 = new Vertex3D((float)p3.x, (float)p3.y, (float)p3.z);

            v0.SetTexcoord(0, 1);
            v1.SetTexcoord(0, 0);
            v2.SetTexcoord(1, 0);
            v3.SetTexcoord(1, 1);

            vec4 textcolor = ConvertColor(backcolor);
            textcolor.w = -1;   //字体所在矩形
            v0.SetColor(textcolor);
            v1.SetColor(textcolor);
            v2.SetColor(textcolor);
            v3.SetColor(textcolor);

            SetPolygonMode(VkPolygonMode.VK_POLYGON_MODE_FILL);

            BeginTriangles();
            AddPoint(v0);
            AddPoint(v1);
            AddPoint(v2);
            AddPoint(v3);
            AddPointIndex(0);
            AddPointIndex(3);
            AddPointIndex(2);
            AddPointIndex(1);
            AddPointIndex(0);
            AddPointIndex(2);
            EndTriangles();
            EnableTexture(false);
            PopMatrix();
        }
        //      |
        //      p1-----p2
        //      |      |
        //      p0-----p3-->
        /// <summary>
        /// 绘制文字
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="color">颜色</param>
        /// <param name="p1">起始位置</param>
        /// <param name="p2">结束位置</param>
        /// <param name="direct">字体顶端单位方向向量</param>        
        public override void DrawString(string text, Font font, Color color, 
                                        Vector64 start, Vector64 end, Vector64 direct,
                                        Color transparent,
                                        TextHorizontalAlignment horAlignment = TextHorizontalAlignment.Left,
                                        TextVerticalAlignment verAlignment = TextVerticalAlignment.Center )
        {
           
            PushMatrix();
            
            BitmapString bm = new BitmapString(text, font, color, transparent);
            EnableTexture(true);                   
            BindTexture( bm.Draw() );

            double width = start.Distance(end);
            double height = width * textureBitmap.Height / (double)textureBitmap.Width;
            Vector64 p0,p1,p2, p3;
            if (verAlignment == TextVerticalAlignment.Top)
            {
                p0 = start - height * direct;
                p3 = end - height * direct;
                p1 = start;
                p2 = end;
            }
            else if (verAlignment == TextVerticalAlignment.Bottom)
            {
                p0 = start;
                p3 = end;
                p1 = p0 + height * direct;
                p2 = p3 + height * direct;
            }
            else //if (verAlignment == TextAlignment.Center)
            {
                p0 = start - 0.5 * height * direct;
                p3 = end - 0.5 * height * direct;
                p1 = start + 0.5 * height * direct;
                p2 = end + 0.5 * height * direct;
            }
            Vertex3D v0 = new Vertex3D((float)p0.x, (float)p0.y, (float)p0.z);
            Vertex3D v1 = new Vertex3D((float)p1.x, (float)p1.y, (float)p1.z);
            Vertex3D v2 = new Vertex3D((float)p2.x, (float)p2.y, (float)p2.z);
            Vertex3D v3 = new Vertex3D((float)p3.x, (float)p3.y, (float)p3.z);            

            v0.SetTexcoord(0, 1);
            v1.SetTexcoord(0, 0);
            v2.SetTexcoord(1, 0);
            v3.SetTexcoord(1, 1);

            vec4 textcolor = ConvertColor(transparent);
            textcolor.w = -1;//字体所在矩形

            v0.SetColor(textcolor);
            v1.SetColor(textcolor);
            v2.SetColor(textcolor);
            v3.SetColor(textcolor);
            
            SetPolygonMode(VkPolygonMode.VK_POLYGON_MODE_FILL);

            BeginTriangles();
            AddPoint(v0);
            AddPoint(v1);
            AddPoint(v2);
            AddPoint(v3);
            AddPointIndex(0);
            AddPointIndex(3);
            AddPointIndex(2);
            AddPointIndex(1);
            AddPointIndex(0);
            AddPointIndex(2);
            EndTriangles(); 
            EnableTexture(false);
            PopMatrix();           
        }

        public override void DrawLine(Vertex3D p1, Vertex3D p2, bool resetColor = true)
        {
            if (resetColor)
            {
                p1.color = curColor;
                p2.color = p1.color;
            }
            Begin(DrawingPrimitive.LINE_LIST);
            Vertex(p1); Vertex(p2);
            VertexIndex(0); VertexIndex(1);
            End();
        }
        
        public override void DrawBox(GridBox box)
        {
            BeginBoxes();
            BoxMemory(box);
            EndBoxes();
        }
        // draw multiple Boxes,xlen ,ylen,zlen are edge lengths
        public bool DrawBox(Vertex3D[] points, float xlen, float ylen, float zlen)
        {
            //  top  north
            //   |  /p4-------p3
            //   | /        / 
            // p1|/-------p2--->east

            //      p4--------p3 
            //      |         |
            //   p8 |     p7  |
            //   |  /p1---|---p2
            //   | /      | / 
            // p5|/-------p6--->east 
            int n = points.Length;
            verticesArray = new Vertex3D[n * 8];
            indicesArray = new int[36 * n];
            if (verticesArray == null || indicesArray == null)
            {
                errMessage = "no enough memory to create buffer.";
                return false;
            }
            float x, y, z;
            int id = 0;
            int[] ids = new int[] { 3,2,1,1,0,3,
                                    4,5,6,6,7,4,
                                    1,2,6,7,6,1,
                                    3,0,4,4,7,3,
                                    2,3,7,7,6,2,
                                    0,1,5,5,4,0 };

            Begin(DrawingPrimitive.TRIANGLE_LIST);

            for (int i = 0; i < n; i++)
            {
                x = points[i].pos.x;
                y = points[i].pos.y;
                z = points[i].pos.z;
                Vertex3D p1 = CreateVertex(x - xlen / 2, y - ylen / 2, z - zlen / 2);
                Vertex3D p2 = CreateVertex(x + xlen / 2, y - ylen / 2, z - zlen / 2);
                Vertex3D p3 = CreateVertex(x + xlen / 2, y + ylen / 2, z - zlen / 2);
                Vertex3D p4 = CreateVertex(x - xlen / 2, y + ylen / 2, z - zlen / 2);
                Vertex3D p5 = CreateVertex(x - xlen / 2, y - ylen / 2, z + zlen / 2);
                Vertex3D p6 = CreateVertex(x + xlen / 2, y - ylen / 2, z + zlen / 2);
                Vertex3D p7 = CreateVertex(x + xlen / 2, y + ylen / 2, z + zlen / 2);
                Vertex3D p8 = CreateVertex(x - xlen / 2, y + ylen / 2, z + zlen / 2);
                p1.SetNormal(-1, -1, -1); p2.SetNormal(1, -1, -1);
                p3.SetNormal(1, 1, -1); p4.SetNormal(-1, 1, -1);
                p5.SetNormal(-1, -1, 1); p6.SetNormal(1, -1, 1);
                p7.SetNormal(1, 1, 1); p8.SetNormal(-1, 1, 1);
                p1.color = p2.color = points[i].color;
                p3.color = p4.color = points[i].color;
                p5.color = p6.color = points[i].color;
                p7.color = p8.color = points[i].color;

                for (int j = 0; j < ids.Length; j++)
                    indicesArray[i * ids.Length + j] = id + ids[j];

                verticesArray[id++] = p1; verticesArray[id++] = p2;
                verticesArray[id++] = p3; verticesArray[id++] = p4;
                verticesArray[id++] = p5; verticesArray[id++] = p6;
                verticesArray[id++] = p7; verticesArray[id++] = p8;

            }

            return End();
        }

        //DrawBox and auto generate texcoord
        public void DrawBox(float x, float y, float z, float xlen, float ylen, float zlen)
        {
            //  top  north
            //   |  /p4-------p3
            //   | /        / 
            // p1|/-------p2--->east            
            Vertex3D p1 = CreateVertex(x - xlen / 2, y - ylen / 2, z - zlen / 2, 0, 1);
            Vertex3D p2 = CreateVertex(x + xlen / 2, y - ylen / 2, z - zlen / 2, 1, 1);
            Vertex3D p3 = CreateVertex(x + xlen / 2, y + ylen / 2, z - zlen / 2, 1, 0);
            Vertex3D p4 = CreateVertex(x - xlen / 2, y + ylen / 2, z - zlen / 2, 0, 0);
            Vertex3D p5 = CreateVertex(x - xlen / 2, y - ylen / 2, z + zlen / 2, 1, 1);
            Vertex3D p6 = CreateVertex(x + xlen / 2, y - ylen / 2, z + zlen / 2, 0, 1);
            Vertex3D p7 = CreateVertex(x + xlen / 2, y + ylen / 2, z + zlen / 2, 0, 0);
            Vertex3D p8 = CreateVertex(x - xlen / 2, y + ylen / 2, z + zlen / 2, 1, 0);
            //      p4--------p3 
            //      |         |
            //   p8 |     p7  |
            //   |  /p1---|---p2
            //   | /      | / 
            // p5|/-------p6--->east          

            p2.SetNormal(1, 0, 0); p3.SetNormal(1, 0, 0);
            p6.SetNormal(1, 0, 1); p7.SetNormal(1, 0, 1);
            p5.SetNormal(-1, 0, 1); p8.SetNormal(-1, 0, 1);
            p1.SetNormal(-1, 0, 0); p4.SetNormal(-1, 0, 0);
            Begin(DrawingPrimitive.TRIANGLE_STRIP);
            Vertex(p1); Vertex(p2); Vertex(p3); Vertex(p4);
            Vertex(p5); Vertex(p6); Vertex(p7); Vertex(p8);
            VertexIndex(1); VertexIndex(2);
            VertexIndex(5); VertexIndex(6);
            VertexIndex(4); VertexIndex(7);
            VertexIndex(0); VertexIndex(3);
            End();
            p1.texCoord = new vec2(0, 0);
            p2.texCoord = new vec2(0, 1);
            p3.texCoord = new vec2(1, 1);
            p4.texCoord = new vec2(1, 0);
            p5.texCoord = new vec2(1, 0);
            p6.texCoord = new vec2(1, 1);
            p7.texCoord = new vec2(0, 1);
            p8.texCoord = new vec2(0, 0);
            //      p4--------p3 
            //      |         |
            //   p8 |     p7  |
            //   |  /p1---|---p2
            //   | /      | / 
            // p5|/-------p6--->east   
            p5.SetNormal(0, -1, 0); p6.SetNormal(0, -1, 0);
            p1.SetNormal(0, -1, -1); p2.SetNormal(0, -1, -1);
            p4.SetNormal(0, 1, -1); p3.SetNormal(0, 1, -1);
            p8.SetNormal(0, 1, 0); p7.SetNormal(0, 1, 0);
            Begin(DrawingPrimitive.TRIANGLE_STRIP);
            Vertex(p1); Vertex(p2); Vertex(p3); Vertex(p4);
            Vertex(p5); Vertex(p6); Vertex(p7); Vertex(p8);
            VertexIndex(4); VertexIndex(5);
            VertexIndex(0); VertexIndex(1);
            VertexIndex(3); VertexIndex(2);
            VertexIndex(7); VertexIndex(6);
            End();
        }
        public override void DrawBoxOutline(GridBox box)
        {
            Vertex3D[] p = new Vertex3D[8];

            for (int i = 0; i < 8; i++)
                p[i] = toVertex3D(box.points[i]);

            if (box.colors == null)
                for (int i = 0; i < 8; i++)
                    p[i].SetColor(curColor);
            else for (int i = 0; i < 8; i++)
                    p[i].SetColor(box.colors[i]);

            Begin(DrawingPrimitive.LINE_STRIP);
            Vertex(p[0]); Vertex(p[1]); Vertex(p[2]); Vertex(p[3]);
            VertexIndex(0); VertexIndex(1);
            VertexIndex(2); VertexIndex(3);
            VertexIndex(0);
            End();
            Begin(DrawingPrimitive.LINE_STRIP);
            Vertex(p[4]); Vertex(p[5]); Vertex(p[6]); Vertex(p[7]);
            VertexIndex(0); VertexIndex(1);
            VertexIndex(2); VertexIndex(3);
            VertexIndex(0);
            End();

            DrawLine(p[0], p[4], false);
            DrawLine(p[1], p[5], false);
            DrawLine(p[2], p[6], false);
            DrawLine(p[3], p[7], false);
        }

        public override void DrawBox(double x, double y, double z,
                                     double xlen, double ylen, double zlen, 
                                     vec4[] _colors = null)
        {
            GridBox box = new GridBox();
            box.Create(x, y, z, xlen, ylen, zlen, _colors);
            DrawBox(box);
            box.Destroy();
        }

        public override void DrawBoxOutline(double x, double y, double z, 
                                            double xlen, double ylen, double zlen, 
                                            vec4[] _colors = null)
        {
            GridBox box = new GridBox();
            box.Create(x, y, z, xlen, ylen, zlen, _colors);
            DrawBoxOutline(box);
            box.Destroy();
        }        
        
        public override void BeginBoxes()
        {
            Begin(DrawingPrimitive.TRIANGLE_LIST);
        }
        public override void EndBoxes()
        {
            End();
        }
        public override void BoxMemory(GridBox box)
        {
            //      |(y)
            //      p3--------p2 
            //      |         |
            //   p7 |     p6  |
            //   |  /p0---|---p1--->(x)
            //   | /      | / 
            // p4|/-------p5--->east 
            //   / (z)
            if (box.points == null || box.faces == null) return;
            int[] up = new int[] { 2, 3, 7, 7, 6, 2 };
            int[] down = new int[] { 0, 1, 5, 5, 4, 0 };
            int[] left = new int[] { 3, 0, 4, 4, 7, 3 };
            int[] right = new int[] { 1, 2, 6, 6, 5, 1 };
            int[] front = new int[] { 4, 5, 6, 6, 7, 4 };
            int[] back = new int[] { 3, 2, 1, 1, 0, 3 };

            int i;

            Vertex3D[] points = new Vertex3D[box.points.Length];
            for (i = 0; i < points.Length; i++)
            {
                points[i] = new Vertex3D(box.points[i].x, box.points[i].y, box.points[i].z);
            }
            int start = pVertics.Count;

            if (box.colors == null)
            {
                for (i = 0; i < 8; i++)
                    points[i].SetColor(curColor);
            }
            else
            {
                for (i = 0; i < box.colors.Length && i < 8; i++)
                    points[i].SetColor(box.colors[i]);
            }

            points[0].SetNormal(0, 0, 0); points[1].SetNormal(0, 0, 0);
            points[2].SetNormal(0, 0, 0); points[3].SetNormal(0, 0, 0);
            points[4].SetNormal(0, 0, 0); points[5].SetNormal(0, 0, 0);
            points[6].SetNormal(0, 0, 0); points[7].SetNormal(0, 0, 0);

            if (box.faces[0])    //up
            {
                for (i = 0; i < up.Length; i++) pIndices.Add(up[i] + start);
                points[2].AddNormal(0, 1, 0); points[3].AddNormal(0, 1, 0);
                points[6].AddNormal(0, 1, 0); points[7].AddNormal(0, 1, 0);
            }
            if (box.faces[1])    //down
            {
                for (i = 0; i < down.Length; i++) pIndices.Add(down[i] + start);
                points[0].AddNormal(0, -1, 0); points[1].AddNormal(0, -1, 0);
                points[4].AddNormal(0, -1, 0); points[5].AddNormal(0, -1, 0);
            }
            if (box.faces[2])    //left
            {
                for (i = 0; i < left.Length; i++) pIndices.Add(left[i] + start);
                points[0].AddNormal(-1, 0, 0); points[3].AddNormal(-1, 0, 0);
                points[4].AddNormal(-1, 0, 0); points[7].AddNormal(-1, 0, 0);
            }
            if (box.faces[3])    //right
            {
                for (i = 0; i < right.Length; i++) pIndices.Add(right[i] + start);
                points[1].AddNormal(1, 0, 0); points[2].AddNormal(1, 0, 0);
                points[5].AddNormal(1, 0, 0); points[6].AddNormal(1, 0, 0);
            }
            if (box.faces[4])    //front
            {
                for (i = 0; i < front.Length; i++) pIndices.Add(front[i] + start);
                points[4].AddNormal(0, 0, 1); points[6].AddNormal(0, 0, 1);
                points[5].AddNormal(0, 0, 1); points[7].AddNormal(0, 0, 1);

            }
            if (box.faces[5])    //back
            {
                for (i = 0; i < back.Length; i++) pIndices.Add(back[i] + start);
                points[0].AddNormal(0, 0, -1); points[1].AddNormal(0, 0, -1);
                points[2].AddNormal(0, 0, -1); points[3].AddNormal(0, 0, -1);
            }
            for (i = 0; i < 8; i++) Vertex(points[i]);

            //release memory
            points = null;
            up = down = null;
            left = right = null;
            front = back = null;
        }
        public override void DrawShpere(vec3 pos, CSphere sp, vec4[] colors = null)
        {
            PushMatrix();
            Translate(pos);
            DrawShpere(sp, colors);
            PopMatrix();
        }
        public override void DrawShpere(Vertex3D p, CSphere sp, vec4[] colors = null)
        {
            PushMatrix();
            DrawShpere(p.pos, sp, colors);
            PopMatrix();
        }
        public override void DrawShpere(CSphere sp, vec4[] colors = null)
        {
            int np = sp.points.Count;
            int na = sp.triangles.Count;
            if (np < 3 || na < 1) return;
            int i1, i2, i3;

            Begin();

            verticesArray = new Vertex3D[np];

            //copy vertics
            for (int i = 0; i < sp.points.Count; i++)
                verticesArray[i] = new Vertex3D(sp.points[i].X, sp.points[i].Y, sp.points[i].Z);

            //copy colors
            if (colors != null)
            {
                for (int i = 0; i < verticesArray.Length; i++)
                {
                    if (colors.Length >= verticesArray.Length)
                        verticesArray[i].color = colors[i];
                    else
                    {
                        verticesArray[i].color = colors[i % colors.Length];
                    }
                }
            }
            else for (int i = 0; i < verticesArray.Length; i++)
                {
                    verticesArray[i].color = curColor;
                }
            //copy indices
            indicesArray = new int[sp.triangles.Count * 3];
            for (int i = 0; i < sp.triangles.Count; i++)
            {
                i1 = sp.triangles[i].x;
                i2 = sp.triangles[i].y;
                i3 = sp.triangles[i].z;
                indicesArray[3 * i] = i1;
                indicesArray[3 * i + 1] = i2;
                indicesArray[3 * i + 2] = i3;
            }

            CalculateNormals(verticesArray, indicesArray);

            End();
        }
        //---------------------------------
        public override void DrawCylinder(vec3 pos, CCylinder sp, vec4[] colors = null)
        {
            PushMatrix();
            Translate(pos);
            DrawCylinder(sp, colors);
            PopMatrix();
        }
        public override void DrawCylinder(Vertex3D p, CCylinder sp, vec4[] colors = null)
        {
            PushMatrix();
            DrawCylinder(p.pos, sp, colors);
            PopMatrix();
        }
        public override bool DrawCylinder(CCylinder sp, vec4[] colors = null)
        {
            TriangleObj tri = sp;
            int np = tri.points.Count;
            if (np < 3)
            {
                errMessage = "no enough points of this object.";
                return false;
            }

            PushMatrix();

            SetCullMode(CullModeEnum.NONE);

            Begin();

            verticesArray = new Vertex3D[np];

            if (verticesArray == null)
            {
                errMessage = "no enough memory.";
                PopMatrix();
                return false;
            }

            //copy vertics
            for (int i = 0; i < tri.points.Count; i++)
            {
                verticesArray[i] = new Vertex3D(tri.points[i].X, tri.points[i].Y, tri.points[i].Z);
                verticesArray[i].texCoord = tri.texCoords[i];
            }
            //copy colors
            if (colors != null)
            {
                for (int i = 0; i < verticesArray.Length; i++)
                {
                    if (colors.Length >= verticesArray.Length)
                        verticesArray[i].color = colors[i];
                    else
                    {
                        verticesArray[i].color = colors[i % colors.Length];
                    }
                }
            }
            else for (int i = 0; i < verticesArray.Length; i++)
                {
                    verticesArray[i].color = tri.color;
                    verticesArray[i].color.w = sp.Alpha;
                }

            indicesArray = new int[sp.horSlices * sp.vertSlices * 6];
            if (indicesArray == null)
            {
                errMessage = "no enough memory.";
                PopMatrix();
                return false;
            }
            int id = 0;
            int i1, i2, i3, i4;
            for (int i = 0; i < sp.vertSlices - 1; i++)
            {
                for (int j = 0; j < sp.horSlices - 1; j++)
                {
                    i1 = j + i * sp.horSlices;
                    i2 = i1 + 1;
                    i3 = i1 + sp.horSlices;
                    i4 = i3 + 1;
                    //  if (j == sp.horSlices - 1)
                    {
                        //      i2 = i * sp.horSlices;
                        //     i4 = i2 + sp.horSlices;
                    }
                    indicesArray[id] = i1; id++;
                    indicesArray[id] = i2; id++;
                    indicesArray[id] = i3; id++;
                    indicesArray[id] = i4; id++;
                    indicesArray[id] = i3; id++;
                    indicesArray[id] = i2; id++;
                }
            }

            CalculateNormals(verticesArray, indicesArray);

            bool ret = End();

            PopMatrix();

            return ret;
        }
        public void BeginDrawQuads()
        {
            //p1 - p4        
            //p2 - p3
            Begin(DrawingPrimitive.TRIANGLE_LIST);
        }
        public void QuadsMemory(Vertex3D p1, Vertex3D p2, Vertex3D p3, Vertex3D p4)
        {
            int id = pVertics.Count;

            vec3 normal = GetNormal(p1.pos, p2.pos, p3.pos);
            p1.SetNormal(normal);
            p2.SetNormal(normal);
            p3.SetNormal(normal);
            p4.SetNormal(normal);

            pVertics.Add(p1);
            pVertics.Add(p2);
            pVertics.Add(p3);
            pVertics.Add(p4);

            pIndices.Add(id);
            pIndices.Add(id + 1);
            pIndices.Add(id + 2);
            pIndices.Add(id + 3);
        }
        public bool EndDrawQuads()
        {
            return End();
        }
        public override vec3 SetTriangleNormal(ref Vertex3D p1, ref Vertex3D p2, ref Vertex3D p3)
        {
            vec3 normal = GetNormal(p1.pos, p2.pos, p3.pos);
            p1.AddNormal(normal);
            p2.AddNormal(normal);
            p3.AddNormal(normal);
            return normal;
        }

        public override void BeginLines()
        {
            Begin(DrawingPrimitive.LINE_LIST);
        }

        private bool toVerticArray()
        {
            if (verticesArray != null) return true;
            verticesArray = new Vertex3D[pVertics.Count];
            if (verticesArray == null)
            {
                errMessage = "no enough memory to alocate vertices buffer.";
                return false;
            }
            //transfer to vertices
            for (int i = 0; i < pVertics.Count; i++)
            {
                verticesArray[i] = pVertics[i];
            }
            pVertics.Clear();
            return true;
        }
        private bool toIndiceArray()
        {
            if (indicesArray == null)
            {
                if (pIndices.Count < 1)
                {
                    errMessage = "no enough points or indices.";
                    pIndices.Clear();
                    return false;
                }
                indicesArray = new int[pIndices.Count];
                if (indicesArray == null)
                {
                    errMessage = "no enough memory to alocate vertices buffer.";
                    pIndices.Clear();
                    return false;
                }
                //transfer to vertices                
                for (int i = 0; i < pIndices.Count; i++)
                {
                    indicesArray[i] = pIndices[i];
                }
                pIndices.Clear();
            }
            return true;
        }
        public override bool EndLines()
        {
            if (verticesArray == null || indicesArray == null)
            {
                if (pVertics.Count < 2 || pIndices.Count < 2)
                {
                    errMessage = "no enough points or indices.";
                    pVertics.Clear();
                    pIndices.Clear();
                    return false;
                }
                verticesArray = new Vertex3D[pVertics.Count];
                indicesArray = new int[pIndices.Count];
                if (verticesArray == null || indicesArray == null)
                {
                    errMessage = "no enough memory to alocate vertices buffer.";
                    pVertics.Clear();
                    pIndices.Clear();
                    return false;
                }
                //transfer to vertices
                for (int i = 0; i < pVertics.Count; i++)
                {
                    verticesArray[i] = pVertics[i];
                }
                for (int i = 0; i < pIndices.Count; i++)
                {
                    indicesArray[i] = pIndices[i];
                }
                pVertics.Clear();
                pIndices.Clear();
            }

            return End();
        }

        public override void BeginTriangles()
        {
            //p1 
            //p2 - p3
            Begin(DrawingPrimitive.TRIANGLE_LIST);
        }
        public override bool EndTriangles()
        {
            if (verticesArray == null || indicesArray == null)
            {
                if (pVertics.Count < 3 || pIndices.Count < 3)
                {
                    errMessage = "no enough points or indices.";
                    pVertics.Clear();
                    pIndices.Clear();
                    return false;
                }
                verticesArray = new Vertex3D[pVertics.Count];
                indicesArray = new int[pIndices.Count];
                if (verticesArray == null || indicesArray == null)
                {
                    errMessage = "no enough memory to alocate vertices buffer.";
                    pVertics.Clear();
                    pIndices.Clear();
                    return false;
                }
                //transfer to vertices
                for (int i = 0; i < pVertics.Count; i++)
                {
                    verticesArray[i] = pVertics[i];
                }
                for (int i = 0; i < pIndices.Count; i++)
                {
                    indicesArray[i] = pIndices[i];
                }
                pVertics.Clear();
                pIndices.Clear();
            }

            CalculateNormals(verticesArray, indicesArray);

            return End();
        }



        //calculate triangles normals
        public void CalculateMeshNormals(Vertex3D[] points, int row, int col)
        {
            if (points.Length != row * col) return;
            vec3 p1, p2, p3;
            int i, j;
            for (i = 0; i < row * col; i++)
                points[i].SetNormal(0, 0, 0);

            for (i = 0; i < row; i++)
            {
                for (j = 0; j < col; j++)
                {
                    p1 = points[i * col + j].pos;
                    if (i < row - 1 && j < col - 1)
                    {
                        p2 = points[i * col + j + col].pos;
                        p3 = points[i * col + j + 1].pos;
                        vec3 nor = GetNormal(p3, p1, p2);
                        points[i * col + j].AddNormal(nor);
                        points[i * col + j + col].AddNormal(nor);
                        points[i * col + j + 1].AddNormal(nor);
                    }
                    if (i < row - 1 && j > 0)
                    {
                        p2 = points[i * col + j + col].pos;
                        p3 = points[i * col + j - 1].pos;
                        vec3 nor = GetNormal(p1, p3, p2);

                        points[i * col + j].AddNormal(nor);
                        points[i * col + j + col].AddNormal(nor);
                        points[i * col + j - 1].AddNormal(nor);

                        p2 = points[i * col + j - 1].pos;
                        p3 = points[i * col + j - 1 + col].pos;
                        nor = GetNormal(p1, p2, p3);

                        points[i * col + j].AddNormal(nor);
                        points[i * col + j - 1].AddNormal(nor);
                        points[i * col + j - 1 + col].AddNormal(nor);
                    }
                    if (i > 0 && j > 0)
                    {
                        p2 = points[i * col + j - col].pos;
                        p3 = points[i * col + j - 1].pos;
                        vec3 nor = GetNormal(p1, p2, p3);
                        points[i * col + j].AddNormal(nor);
                        points[i * col + j - col].AddNormal(nor);
                        points[i * col + j - 1].AddNormal(nor);
                    }
                    if (i > 0 && j < col - 1)
                    {
                        p2 = points[i * col + j - col].pos;
                        p3 = points[i * col + j - col + 1].pos;
                        vec3 nor = GetNormal(p1, p3, p2);
                        points[i * col + j].AddNormal(nor);
                        points[i * col + j - col].AddNormal(nor);
                        points[i * col + j - col + 1].AddNormal(nor);

                        p2 = points[i * col + j + 1].pos;
                        p3 = points[i * col + j - col + 1].pos;
                        nor = GetNormal(p1, p2, p3);
                        points[i * col + j].AddNormal(nor);
                        points[i * col + j + 1].AddNormal(nor);
                        points[i * col + j - col + 1].AddNormal(nor);
                    }
                }
            }
        }
        public override void DrawMesh(Vertex3D[] points, int row, int col,
                                bool autoNormal = true,
                                bool resetColor = true,
                                bool autoTexCoord = false)
        {
            //0 1 2 ...     col-1
            //--------------> row 0
            //col .....     2*col-1
            //--------------> row 1
            //2*col .....   3*col-1
            //--------------> row 2
            // points.Length = row *col

            if (points.Length != row * col) return;

            //verticesArray = points;
            if (resetColor)
            {
                vec4 color = curColor;
                for (int i = 0; i < points.Length; i++)
                    points[i].color = color;
            }
            if (autoTexCoord)
            {
                float y = 1.0f / (row - 1);
                float x = 1.0f / (col - 1);

                for (int i = 0; i < row; i++)
                {
                    for (int j = 0; j < col; j++)
                    {
                        points[i * col + j].texCoord.x = x * j;
                        points[i * col + j].texCoord.y = y * i;
                    }
                }
            }
            if (autoNormal)
            {
                CalculateMeshNormals(points, row, col);
            }
            int id;
            for (int i = 0; i < row - 1; i++)
            {
                int vid = 0;
                Begin(DrawingPrimitive.TRIANGLE_STRIP);
                for (int j = 0; j < col; j++)
                {
                    id = i * col + j;
                    Vertex(points[id]);
                    VertexIndex(vid);

                    id = i * col + j + col;
                    Vertex(points[id]);
                    VertexIndex(vid + 1);
                    vid += 2;
                }
                End();
            }
            verticesArray = null;
            indicesArray = null;
            uniformBuffer = null;
        }


        public override void PushMatrix()
        {
            base.PushMatrix();

            pipelineInfoMatrix.Add(CopyPipelineInfo(pipelineInfo));
        }
        public override void PopMatrix()
        {
            base.PopMatrix();

            int n = pipelineInfoMatrix.Count;
            if (n > 0)
            {
                pipelineInfo = CopyPipelineInfo(pipelineInfoMatrix[n - 1]);
                pipelineInfoMatrix.RemoveAt(n - 1);
            }
        }

        public override void LoadIdentity()
        {
            m_translate = new vec3(0, 0, 0);
            m_scale = new vec3(1, 1, 1);
            m_rotate = new vec3(0, 0, 0);//x,y,z axe angle
            m_eye = new vec3(0, 0, 2);
            m_center = new vec3(0, 0, 0);
            m_up = new vec3(0, 1, 0);

            m_modelMatrix.model = new mat4(1.0f);
            //glm.rotate(new mat4(1.0f),
            //glm.radians(0.0f),
            //new vec3(0.0f, 1.0f, 0.0f));

            //m_modelMatrix.normal = glm.inverse(m_modelMatrix.model);


            m_modelMatrix.view = glm.lookAt(m_eye, m_center, m_up);
            m_modelMatrix.proj = glm.perspective(glm.radians(45.0f),
                                         swapChainExtent.width / (float)swapChainExtent.height,
                                         0.001f, 1000.0f);

            m_modelMatrix.eyepos = new vec4(m_eye, 1.0f);

            m_modelMatrix.lights[0].Enable = true;
        }




        public override bool Blend
        {
            get { return _blending; }
            set
            {
                if (_blending == value) return;
                _blending = value;
            }
        }

        //
        public CVulkan()
        {
            validationLayers = new string[]
           {
                "VK_LAYER_LUNARG_standard_validation",
               //"VK_LAYER_LUNARG_api_dump",
               //"VK_LAYER_LUNARG_core_validation",
               //"VK_LAYER_LUNARG_parameter_validation"
           };
            deviceExtensions = new string[]
            {
                 "VK_KHR_swapchain",
            };
            LoadIdentity();
            m_Semaphore = new Semaphore(1, 1);
        }
        private void CreateDefaultTextureSamplerInfo()
        {
            textureSamplerInfo = new VkSamplerCreateInfo();
            textureSamplerInfo.magFilter = VkFilter.VK_FILTER_LINEAR;
            textureSamplerInfo.minFilter = VkFilter.VK_FILTER_LINEAR;
            textureSamplerInfo.addressModeU = VkSamplerAddressMode.VK_SAMPLER_ADDRESS_MODE_REPEAT;
            textureSamplerInfo.addressModeV = VkSamplerAddressMode.VK_SAMPLER_ADDRESS_MODE_REPEAT;
            textureSamplerInfo.addressModeW = VkSamplerAddressMode.VK_SAMPLER_ADDRESS_MODE_REPEAT;
            textureSamplerInfo.anisotropyEnable = false;
            textureSamplerInfo.maxAnisotropy = 1;
            textureSamplerInfo.borderColor = VkBorderColor.VK_BORDER_COLOR_INT_OPAQUE_BLACK;
            textureSamplerInfo.unnormalizedCoordinates = false;
            textureSamplerInfo.compareEnable = false;
            textureSamplerInfo.compareOp = VkCompareOp.VK_COMPARE_OP_ALWAYS;
            textureSamplerInfo.mipmapMode = VkSamplerMipmapMode.VK_SAMPLER_MIPMAP_MODE_LINEAR;
            textureSamplerInfo.mipLodBias = 0.0f;
            textureSamplerInfo.minLod = 0.0f;
            textureSamplerInfo.maxLod = 0.0f;
        }
        public string GetErrMsg()
        {
            return errMessage;
        }
        public VkPhysicalDevice[] GetPhysicalDevices()
        {
            EnumeratePhysicalDevices();
            return physicalDevices;
        }
#pragma warning disable CS0114 // '“CVulkan.GetMax2DTextureImageSize()”隐藏继承的成员“CGraphic3D.GetMax2DTextureImageSize()”。若要使当前成员重写该实现，请添加关键字 override。否则，添加关键字 new。
        public int GetMax2DTextureImageSize()
#pragma warning restore CS0114 // '“CVulkan.GetMax2DTextureImageSize()”隐藏继承的成员“CGraphic3D.GetMax2DTextureImageSize()”。若要使当前成员重写该实现，请添加关键字 override。否则，添加关键字 new。
        {
            /*
            VkPhysicalDeviceProperties deviceProperties;
            VulkanAPI.vkGetPhysicalDeviceProperties(physicalDevice, out deviceProperties);
            VkPhysicalDeviceLimits limit = deviceProperties.limit;
            */
            return 16384;
        }
        public string[] GetPhysicalDevicesInfo()
        {
            if (!EnumeratePhysicalDevices()) return null;
            string[] infos = new string[physicalDevices.Length];
            for (int i = 0; i < physicalDevices.Length; i++)
            {
                VkPhysicalDeviceProperties deviceProperties;
                VulkanAPI.vkGetPhysicalDeviceProperties(physicalDevices[i], out deviceProperties);
                infos[i] = deviceProperties.ToString();
            }
            return infos;
        }
        public bool SelectPhysicalDevices(int index)
        {
            if (!EnumeratePhysicalDevices()) return false;
            if (index < 0 || index > physicalDevices.Length) return false;
            physicalDevice = physicalDevices[index];
            return true;
        }
        private bool checkValidationLayerSupport()
        {
            VulkanAPI.vkEnumerateInstanceLayerProperties(out availableLayers);

            availableLayersArray = new string[availableLayers.Length];
            for (int i = 0; i < availableLayers.Length; i++)
                availableLayersArray[i] = availableLayers[i].layerName;

            //check if the validationLayers exist in the availableLayers
            for (int i = 0; i < validationLayers.Length; i++)
            {
                bool layerFound = false;
                for (int j = 0; j < availableLayers.Length; j++)
                {
                    if (validationLayers[i] == availableLayersArray[j])
                    {
                        layerFound = true;
                        break;
                    }
                }
                if (!layerFound)
                {
                    return false;
                }
            }
            return true;
        }
        private string[] getRequiredExtensions()
        {
            List<string> extensions = new List<string>(Glfw.GetRequiredInstanceExtensions());
#pragma warning disable CS0162 // 检测到无法访问的代码
            if (enableValidationLayers)extensions.Add("VK_EXT_debug_report");
#pragma warning restore CS0162 // 检测到无法访问的代码
            requiredExtensions = extensions.ToArray();
            return requiredExtensions;
        }
        private int VK_MAKE_VERSION(int major, int minor, int patch)
        {
            return (((major) << 22) | ((minor) << 12) | (patch));
        }
        public bool CreateVKInstance()
        {
            VkApplicationInfo appInfo = new VkApplicationInfo();
            appInfo.ApplicationName = windowTitle;
            appInfo.ApplicationVersion = (uint)VK_MAKE_VERSION(1, 0, 0);//4194304;
            appInfo.EngineName = "no egine";
            appInfo.EngineVersion = (uint)VK_MAKE_VERSION(1, 0, 0);
            
            VkInstanceCreateInfo createInfo = new VkInstanceCreateInfo();
            string[] extensions = getRequiredExtensions();
            createInfo.ApplicationInfo = appInfo;
            createInfo.EnabledExtensionNames = extensions;
            createInfo.EnabledLayerNames = null;
            if (enableValidationLayers && !checkValidationLayerSupport())
            {
                errMessage = "validation layers requested, but not available";
                return false;
            }

            if (enableValidationLayers)
            {
                createInfo.EnabledExtensionNames = extensions;
                // it would cause CreateShaderModule failed???
                createInfo.EnabledLayerNames = validationLayers;
            }

            SharpVulkan.VkResult result = VulkanAPI.vkCreateInstance(createInfo, out vkInstance);
            if (result != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "create vulkan instance error.";
                return false;
            }

            errMessage = "create vulkan instance success.";
            return true;
        }

        public bool IsDeviceSupport()
        {
            if (vkInstance == null)
            {
                if (!CreateVKInstance())
                {
                    return false;
                }
            }

            if (vulkanSurface == null)
            {
                if (!createSurface())
                {
                    return false;
                }
            }

            if (!pickPhysicalDevice())
            {
                if (vkInstance != null)
                {
                    VulkanAPI.vkDestroyInstance(vkInstance);
                    vkInstance = null;
                }
                return false;
            }
            
            return true;
        }
        private VkDebugReportCallbackEXT debugReportCallback;
        private vkDebugReportCallback debugReport;
        bool debugCallback(VkDebugReportFlagBitsEXT flags,
                                          VkDebugReportObjectTypeEXT objectType,
                                          ulong @object,
                                          UIntPtr location,
                                          int messageCode,
                                          string layerPrefix,
                                          string message,
                                          IntPtr pUserData)
        {
            //System.Console.WriteLine("DebugReport layer: {0} message: {1}", layerPrefix, message);
            AddToReports(layerPrefix + ":" + message);
            return true;
        }
        private bool setupDebugCallback()
        {
            if (!enableValidationLayers)
            {
                errMessage = "Validation layers disenabled";
                return true;
            }
#pragma warning disable CS0162 // 检测到无法访问的代码
            var createDebugReportCallback = VulkanAPI.vkGetInstanceProcAddr<vkCreateDebugReportCallbackEXT>(vkInstance, "vkCreateDebugReportCallbackEXT");
#pragma warning restore CS0162 // 检测到无法访问的代码
            debugReport = debugCallback;
            var createInfo = new VkDebugReportCallbackCreateInfoEXT
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_DEBUG_REPORT_CALLBACK_CREATE_INFO_EXT,
                flags = VkDebugReportFlagBitsEXT.VK_DEBUG_REPORT_ERROR_BIT_EXT,// |
                       // VkDebugReportFlagBitsEXT.VK_DEBUG_REPORT_WARNING_BIT_EXT |
                       // VkDebugReportFlagBitsEXT.VK_DEBUG_REPORT_PERFORMANCE_WARNING_BIT_EXT,
                pfnCallback = debugReport
            };

            // it always return false????? but it works
            if (!createDebugReportCallback(vkInstance.NativeHandle, ref createInfo, IntPtr.Zero, out debugReportCallback))
            {
                errMessage = "failed to set up debug callback!";
                //return false;     //error jian debug
            }

            return true;
        }
        // [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        //private unsafe delegate IntPtr glfwGetWin32Window(GLFWwindow window);

        // to check the device is suitable
        private bool createSurface()
        {
            VkWin32SurfaceCreateInfoKHR createInfo = new VkWin32SurfaceCreateInfoKHR();

            if (windowHandle == null || windowHandle == IntPtr.Zero)//create glfw window
            {
                glfw3.Glfw.DefaultWindowHints();
                //glfw3.Glfw.WindowHint(0x00022001, 0);
                // glfw3.Glfw.WindowHint(0x00020003, 0);                
                glfwWindow = glfw3.Glfw.CreateWindow(windowWidth, windowHeight, windowTitle, null, null);
                createInfo.hwnd = glfwWindow.__Instance;// GetWin32Window(glfwWindow);
                createInfo.hinstance = Process.GetCurrentProcess().Handle;
            }
            else
            {
                createInfo.hwnd = windowHandle;
                createInfo.hinstance = Process.GetCurrentProcess().Handle;
            }
            if (VulkanAPI.vkCreateWin32SurfaceKHR(vkInstance, ref createInfo, out vulkanSurface) != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "Create surface failed.";
                return false;
            }

            errMessage = "Created surface successfully.";
            return true;
        }
        private bool EnumeratePhysicalDevices()
        {
            if (physicalDevices == null)
            {
                VulkanAPI.vkEnumeratePhysicalDevices(vkInstance, out physicalDevices);
                if (physicalDevices.Length == 0)
                {
                    errMessage = "failed to Enumerate PhysicalDevices.";
                    return false;
                }
            }
            return true;
        }

        private bool pickPhysicalDevice()
        {
            if (graphicDevice.ToLower().Contains("auto") )
            {
                return pickPhysicalDeviceOnScore();
            }
            else
            {
                return pickPhysicalDeviceOnName(graphicDevice);
            }
        }

        private bool pickPhysicalDeviceOnScore()
        {
            // Use an ordered map to automatically sort candidates by increasing score
            physicalDevice = null;
            if (!EnumeratePhysicalDevices()) return false;
            ulong maxscore = 0;
            for (int i = 0; i < physicalDevices.Length; i++)
            {
                // device is not suitable
                if (!IsDeviceSuitable(physicalDevices[i])) continue;
                // rank the device on score
                ulong score = RateDeviceScore(physicalDevices[i]);
                if (score > maxscore)
                {
                    physicalDevice = physicalDevices[i];
                    maxscore = score;
                }
            }
            if (physicalDevice == null)
            {
                errMessage = "failed to find suitable GPUs!";
                return false;
            }
            
            currentGraphicDevice = GetGraphicsDeviceInfo(physicalDevice);

            errMessage = "picked GPU successfully!";
            return true;
        }
        private bool pickPhysicalDeviceOnName(string deviceName)
        {
            // Use an ordered map to automatically sort candidates by increasing score
            physicalDevice = null;
            if ( !EnumeratePhysicalDevices() ) return false;
           
            for (int i = 0; i < physicalDevices.Length; i++)
            {
               GraphicDeviceInfo info = GetGraphicsDeviceInfo(physicalDevices[i]);
                if (info == null) continue;

                if( info.Name.ToLower().Contains(deviceName.ToLower() ) )
                {
                    physicalDevice = physicalDevices[i];
                    currentGraphicDevice = info;
                    // device is not suitable
                    if ( !IsDeviceSuitable(physicalDevices[i]) ) 
                    {
                        errMessage = "device selected is not suitable for vulkan.\r\n" + info.Name ;
                        return false;
                    }
                    return true;
                }
            }

            if (physicalDevice == null)
            {
                errMessage = "failed to find a suitable graphic device for vulkan!";
                return false;
            }            

            errMessage = "picked GPU successfully!";
            return true;
        }
        public GraphicDeviceInfo GetGraphicsDeviceInfo(VkPhysicalDevice device)
        {
            VkPhysicalDeviceProperties deviceProperties;
            VulkanAPI.vkGetPhysicalDeviceProperties(device, out deviceProperties);
            string properties = deviceProperties.ToString();
            // Discrete GPUs have a significant performance advantage            
            return GetGraphicsDeviceInfo(properties);
        }
        public override string GetGraphicName()
        {
            if (currentGraphicDevice != null)
                return currentGraphicDevice.Name;
            else return base.GetGraphicName();
        }
        public override string GetGraphicsInfoString()
        {
            if (currentGraphicDevice != null)
                return currentGraphicDevice.toString();
            else return base.GetGraphicsInfoString();
        }
        // get the rate score on device
        // get the rate score of the device
        public ulong RateDeviceScore(VkPhysicalDevice device)
        {
            ulong score = 0;
            VkPhysicalDeviceProperties deviceProperties;
            VulkanAPI.vkGetPhysicalDeviceProperties(device, out deviceProperties);
            string properties = deviceProperties.ToString();
            // Discrete GPUs have a significant performance advantage
            if (properties.IndexOf("VK_PHYSICAL_DEVICE_TYPE_DISCRETE_GPU") >= 0)
            {
                score += 1000;
            }
            
            //从字符串中检索出设备信息。（好慢，有待提高效率）
            GraphicDeviceInfo info = GetGraphicsDeviceInfo(properties);
            if (info != null)
            {
                score += (uint)info.MemorySize;
            }
            // below not support on this binding
            // Maximum possible size of textures affects graphics quality
            // score += deviceProperties.limits.maxImageDimension2D;
            // Application can't function without geometry shaders            
            /* Feature操作不支持
            VkPhysicalDeviceFeatures deviceFeatures;
            VulkanAPI.vkGetPhysicalDeviceFeatures(device, out deviceFeatures);
            string features = deviceFeatures.ToString();
            //if (!deviceFeatures.geometryShader)
            {
               return 0;
            }
            */
            return score;
        }
        // is the device support vulkan
        public bool IsDeviceSuitable(VkPhysicalDevice device)
        {
            QueueFamilyIndices indices = findQueueFamilies(device);
            bool extensionsSupported = checkDeviceExtensionSupport(device);
            bool swapChainAdequate = false;
            if (extensionsSupported)
            {
                SwapChainSupportDetails swapChainSupport = querySwapChainSupport(device);
                swapChainAdequate = swapChainSupport.formats.Length > 0 &&
                                    swapChainSupport.presentModes.Length > 0;
            }
            // not supported
            //VkPhysicalDeviceFeatures supportedFeatures; 
            //vkGetPhysicalDeviceFeatures(device, &supportedFeatures);
            //supportedFeatures.samplerAnisotropy;
            return indices.isComplete() && extensionsSupported && swapChainAdequate;
        }
        /*
        private bool IsDeviceSuitable(VkPhysicalDevice device)
        {
            bool suitable = true;

            // check the properties
            VkPhysicalDeviceProperties deviceProperty;            
            VulkanAPI.vkGetPhysicalDeviceProperties(device, out deviceProperty);            
            string properties = deviceProperty.ToString();
            if (properties.IndexOf("VK_PHYSICAL_DEVICE_TYPE_DISCRETE_GPU") < 0)
                suitable = false;

            //check the features
            VkPhysicalDeviceFeatures deviceFeatures;
            VulkanAPI.vkGetPhysicalDeviceFeatures(device, out deviceFeatures);
            string features = deviceFeatures.ToString();

            //check the queue families
            VkQueueFamilyProperties[] ques;
            VulkanAPI.vkGetPhysicalDeviceQueueFamilyProperties(device, out ques);
            queueFamilyIndices = -1;
            for(int i=0;i<ques.Length;i++)
            {
                if (  ( ques[i].queueFlags & VkQueueFlags.VK_QUEUE_GRAPHICS_BIT ) > 0
                       && ques[i].queueCount > 0)
                {
                    queueFamilyIndices = i;
                    break;
                }
            }
            if (queueFamilyIndices < 0) suitable = false;

            //check swap chain support
            QueueFamilyIndices indices = findQueueFamilies(device);
            bool extensionsSupported = checkDeviceExtensionSupport(device);            
            bool swapChainAdequate = false;

            if (extensionsSupported)
            {
                SwapChainSupportDetails swapChainSupport = querySwapChainSupport(device);
                swapChainAdequate = swapChainSupport.formats.Length>0 &&
                                    swapChainSupport.presentModes.Length>0 ;
                swapChainAdequate = true; //jian debug
            }

            return suitable && indices.isComplete() && extensionsSupported && swapChainAdequate;
        }
        */
        private bool checkDeviceExtensionSupport(VkPhysicalDevice device)
        {
            VkExtensionProperties[] availableExtensions;
            VulkanAPI.vkEnumerateDeviceExtensionProperties(device, null, out availableExtensions);
            for (int i = 0; i < deviceExtensions.Length; i++)
            {
                bool swapChainFound = false;
                for (int j = 0; j < availableExtensions.Length; j++)
                {
                    if (deviceExtensions[i] == availableExtensions[j].extensionName)
                    {
                        swapChainFound = true;
                        break;
                    }
                }
                if (!swapChainFound)
                {
                    return false;
                }
            }
            return true;
        }
        private QueueFamilyIndices findQueueFamilies(VkPhysicalDevice device)
        {
            VkQueueFamilyProperties[] queueFamilies;
            VulkanAPI.vkGetPhysicalDeviceQueueFamilyProperties(device, out queueFamilies);
            QueueFamilyIndices indices = new QueueFamilyIndices();
            for (int i = 0; i < queueFamilies.Length; i++)
            {
                if ((queueFamilies[i].queueCount > 0) &&
                    (queueFamilies[i].queueFlags & VkQueueFlags.VK_QUEUE_GRAPHICS_BIT) > 0)
                {
                    indices.graphicsFamily = i;
                }
                VkBool32 presentSupport = false;
                VulkanAPI.vkGetPhysicalDeviceSurfaceSupportKHR(device, (uint)i, vulkanSurface, out presentSupport);
                VkSurfaceCapabilitiesKHR capabilities;
                VulkanAPI.vkGetPhysicalDeviceSurfaceCapabilitiesKHR(device, vulkanSurface, out capabilities);

                if (queueFamilies[i].queueCount > 0 && (bool)presentSupport)
                {
                    indices.presentFamily = i;
                }
                if (indices.isComplete())
                {
                    break;
                }
                i++;
            }
            return indices;
        }
        private SwapChainSupportDetails querySwapChainSupport(VkPhysicalDevice device)
        {
            SwapChainSupportDetails details = new SwapChainSupportDetails();
            VulkanAPI.vkGetPhysicalDeviceSurfaceCapabilitiesKHR(device, vulkanSurface, out details.capabilities);

            //get Surface format
            VulkanAPI.vkGetPhysicalDeviceSurfaceFormatsKHR(device, vulkanSurface, out details.formats);
            //get Presentation mode
            VulkanAPI.vkGetPhysicalDeviceSurfacePresentModesKHR(device, vulkanSurface, out details.presentModes);

            return details;
        }
        VkSurfaceFormatKHR chooseSwapSurfaceFormat(VkSurfaceFormatKHR[] availableFormats)
        {
            VkSurfaceFormatKHR availableFormat = new VkSurfaceFormatKHR();
            if (availableFormats.Length == 1 && availableFormats[0].format == VkFormat.VK_FORMAT_UNDEFINED)
            {
                availableFormat.format = VkFormat.VK_FORMAT_B8G8R8A8_UNORM;
                availableFormat.colorSpace = VkColorSpaceKHR.VK_COLOR_SPACE_SRGB_NONLINEAR_KHR;
                return availableFormat;
            }
            for (int i = 0; i < availableFormats.Length; i++)
            {
                availableFormat = availableFormats[i];
                if (availableFormat.format == VkFormat.VK_FORMAT_B8G8R8A8_UNORM &&
                     availableFormat.colorSpace == VkColorSpaceKHR.VK_COLOR_SPACE_SRGB_NONLINEAR_KHR)
                {
                    return availableFormat;
                }
            }
            return availableFormats[0];
        }
        private VkPresentModeKHR chooseSwapPresentMode(VkPresentModeKHR[] availablePresentModes)
        {
            VkPresentModeKHR bestMode = VkPresentModeKHR.VK_PRESENT_MODE_FIFO_KHR;
            for (int i = 0; i < availablePresentModes.Length; i++)
            {
                if (availablePresentModes[i] == VkPresentModeKHR.VK_PRESENT_MODE_MAILBOX_KHR)
                {
                    return availablePresentModes[i];
                }
                else if (availablePresentModes[i] == VkPresentModeKHR.VK_PRESENT_MODE_IMMEDIATE_KHR)
                {
                    bestMode = availablePresentModes[i];
                }
            }
            return bestMode;
        }
        private VkExtent2D chooseSwapExtent(VkSurfaceCapabilitiesKHR capabilities)
        {
            if (capabilities.currentExtent.width != 0xffffffff)
            {
                return capabilities.currentExtent;
            }
            else
            {
                VkExtent2D actualExtent = new VkExtent2D();
                actualExtent.width = (uint)windowWidth;
                actualExtent.height = (uint)windowHeight;
                actualExtent.width = Math.Max(capabilities.minImageExtent.width,
                                         Math.Min(capabilities.maxImageExtent.width, actualExtent.width));
                actualExtent.height = Math.Max(capabilities.minImageExtent.height,
                                         Math.Min(capabilities.maxImageExtent.height, actualExtent.height));

                return actualExtent;
            }
        }
        private bool createSwapChain()
        {
            SwapChainSupportDetails swapChainSupport = querySwapChainSupport(physicalDevice);
            VkSurfaceFormatKHR surfaceFormat = chooseSwapSurfaceFormat(swapChainSupport.formats);
            VkPresentModeKHR presentMode = chooseSwapPresentMode(swapChainSupport.presentModes);
            VkExtent2D extent = chooseSwapExtent(swapChainSupport.capabilities);

            //textureBitmapFormat = surfaceFormat.format;

            uint imageCount = swapChainSupport.capabilities.minImageCount + 1;
            if (swapChainSupport.capabilities.maxImageCount > 0 &&
                imageCount > swapChainSupport.capabilities.maxImageCount)
            {
                imageCount = swapChainSupport.capabilities.maxImageCount;
            }

            VkSwapchainCreateInfoKHR createInfo = new VkSwapchainCreateInfoKHR();
            createInfo.surface = vulkanSurface;
            createInfo.minImageCount = imageCount;
            createInfo.imageFormat = surfaceFormat.format;
            createInfo.imageColorSpace = surfaceFormat.colorSpace;
            createInfo.imageExtent = extent;
            createInfo.imageArrayLayers = 1;
            createInfo.imageUsage = VkImageUsageFlags.VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT;

            QueueFamilyIndices indices = findQueueFamilies(physicalDevice);
            int[] queueFamilyIndices = new int[]
            {
                indices.graphicsFamily,
                indices.presentFamily
            };
            // get safe pointer of queueFamilyIndices[]
            GCHandle hObject = GCHandle.Alloc(queueFamilyIndices, GCHandleType.Pinned);
            IntPtr pObject = hObject.AddrOfPinnedObject();

            if (indices.graphicsFamily != indices.presentFamily)
            {
                createInfo.imageSharingMode = VkSharingMode.VK_SHARING_MODE_CONCURRENT;
                createInfo.queueFamilyIndexCount = 2;
                createInfo.pQueueFamilyIndices = pObject;
            }
            else
            {
                createInfo.imageSharingMode = VkSharingMode.VK_SHARING_MODE_EXCLUSIVE;
            }

            createInfo.preTransform = swapChainSupport.capabilities.currentTransform;
            createInfo.compositeAlpha = VkCompositeAlphaFlagsKHR.VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR;
            createInfo.presentMode = presentMode;
            createInfo.clipped = VkBool32.True;

            createInfo.oldSwapchain = null;

            if (VulkanAPI.vkCreateSwapchainKHR(device, ref createInfo, out swapChain)
                 != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "failed to create swap chain!";
                return false;
            }
            errMessage = "created swap chain sucessfully!";

            VulkanAPI.vkGetSwapchainImagesKHR(device, swapChain, out swapChainImages);
            swapChainImageFormat = surfaceFormat.format;
            swapChainExtent = extent;
            return true;
        }
        private void CopyVertexBufferTo(Vertex2D[] obj, IntPtr data)
        {
            int len = obj.Length;
            if (len == 0) return;

            int size = Vertex2D.GetSize();
            byte[] bytes = new byte[size];
            int offset = 0;
            for (int i = 0; i < len; i++)
            {
                IntPtr buffer = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(obj[i], buffer, false);
                Marshal.Copy(buffer, bytes, 0, size);
                Marshal.Copy(bytes, 0, data + offset, size);
                offset += size;
            }
        }
        private void CopyVertexBufferTo(Vertex3D[] obj, IntPtr data)
        {
            int len = obj.Length;
            if (len == 0) return;

            int size = Vertex3D.GetSize();
            byte[] bytes = new byte[size];
            int offset = 0;
            for (int i = 0; i < len; i++)
            {
                IntPtr buffer = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(obj[i], buffer, false);
                Marshal.Copy(buffer, bytes, 0, size);
                Marshal.Copy(bytes, 0, data + offset, size);
                offset += size;
            }
        }

        private VkCommandBuffer beginSingleTimeCommands()
        {
            VkCommandBuffer[] combuffers = new VkCommandBuffer[1];
            VkCommandBufferAllocateInfo allocInfo = new VkCommandBufferAllocateInfo();
            allocInfo.commandPool = commandPool;
            allocInfo.level = VkCommandBufferLevel.VK_COMMAND_BUFFER_LEVEL_PRIMARY;
            allocInfo.commandBufferCount = (uint)combuffers.Length;

            VulkanAPI.vkAllocateCommandBuffers(device, ref allocInfo, out combuffers);
            if (combuffers == null)
            {
                errMessage = "vkAllocateCommandBuffers failed.";
                return null;
            }
            VkCommandBuffer commandBuffer = combuffers[0];
            SharpVulkan.VkResult result;
            result = VulkanAPI.vkBeginCommandBuffer(combuffers[0]);
            if (result != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "vkBeginCommandBuffer failed.";
                return null;
            }
            return commandBuffer;
        }
        private bool endSingleTimeCommands(VkCommandBuffer commandBuffer)
        {
            SharpVulkan.VkResult result = VulkanAPI.vkEndCommandBuffer(commandBuffer);
            if (result != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "vkEndCommandBuffer failed.";
                return false;
            }
            VkSubmitInfo submitInfo = new VkSubmitInfo();
            submitInfo.commandBuffers = new[] { commandBuffer };
            result = VulkanAPI.vkQueueSubmit(graphicsQueue, submitInfo, null);
            if (result != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "vkQueueSubmit failed.";
                return false;
            }
            result = VulkanAPI.vkQueueWaitIdle(graphicsQueue);
            if (result != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "vkQueueWaitIdle failed.";
                return false;
            }
            VulkanAPI.vkFreeCommandBuffers(device, commandPool, new[] { commandBuffer });
            return true;
        }
        private bool copyBuffer(VkBuffer srcBuffer, VkBuffer dstBuffer, VkDeviceSize size)
        {
            VkCommandBuffer commandBuffer = beginSingleTimeCommands();
            if (commandBuffer == null) return false;
            VkBufferCopy copyRegion = new VkBufferCopy();
            copyRegion.srcOffset = 0; // Optional
            copyRegion.dstOffset = 0; // Optional
            copyRegion.size = size;
            VulkanAPI.vkCmdCopyBuffer(commandBuffer, srcBuffer, dstBuffer, new[] { copyRegion });
            return endSingleTimeCommands(commandBuffer);
        }

        private int findMemoryType(int typeFilter, VkMemoryPropertyFlags properties)
        {
            VkPhysicalDeviceMemoryProperties memoryProperties;
            VulkanAPI.vkGetPhysicalDeviceMemoryProperties(physicalDevice, out memoryProperties);
            var memoryTypes = memoryProperties.memoryTypes;
            for (int i = 0; i < memoryProperties.memoryTypeCount; i++)
            {
                if (((typeFilter >> i) & 1) == 1 &&
                     (memoryTypes[i].propertyFlags & properties) == properties)
                {
                    return i;
                }
            }
            return -1;
        }
        private bool createBuffer(VkDeviceSize size, VkBufferUsageFlags usage, VkMemoryPropertyFlags properties,
                                   out VkBuffer buffer, out VkDeviceMemory bufferMemory)
        {
            buffer = null;
            bufferMemory = null;
            VkBufferCreateInfo bufferInfo = new VkBufferCreateInfo();
            bufferInfo.size = size;
            bufferInfo.usage = usage;
            bufferInfo.sharingMode = VkSharingMode.VK_SHARING_MODE_EXCLUSIVE;
            if (VulkanAPI.vkCreateBuffer(device, ref bufferInfo, out buffer)
                != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "call vkCreateBuffer error,failed to create buffer.";
                return false;
            }
            // Get memory requirements
            VkMemoryRequirements memRequirements;
            VulkanAPI.vkGetBufferMemoryRequirements(device, buffer, out memRequirements);
            int memoryTypeIndex = findMemoryType((int)memRequirements.memoryTypeBits, properties);
            if (memoryTypeIndex < 0)
            {
                errMessage = "failed to find suitable memory type!";
                return false;
            }
            // allocate memory
            VkMemoryAllocateInfo allocInfo = new VkMemoryAllocateInfo();
            allocInfo.allocationSize = memRequirements.size;
            allocInfo.memoryTypeIndex = (uint)memoryTypeIndex;
            if (VulkanAPI.vkAllocateMemory(device, ref allocInfo, out bufferMemory)
                != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "error to call vkAllocateMemory:failed to allocate buffer memory!";
                return false;
            }
            // bind memory
            if (VulkanAPI.vkBindBufferMemory(device, buffer, bufferMemory, 0)
                 != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "failed to bind buffer memory!";
                return false;
            }
            return true;
        }
        public bool createVertexBuffer(Vertex3D[] points,out VkBuffer buffer,out VkDeviceMemory memory)
        {
            buffer = null;
            memory = null;
            if (points.Length < 1)
            {
                errMessage = "invalid vertices array!";
                return false;
            }

            VkDeviceSize bufferSize = Vertex3D.GetSize() * points.Length;
            VkBuffer stagingBuffer;
            VkDeviceMemory stagingBufferMemory;
            //create source buffer
            if (!createBuffer(bufferSize,
                          VkBufferUsageFlags.VK_BUFFER_USAGE_TRANSFER_SRC_BIT,
                          VkMemoryPropertyFlags.VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT |
                          VkMemoryPropertyFlags.VK_MEMORY_PROPERTY_HOST_COHERENT_BIT,
                          out stagingBuffer,
                          out stagingBufferMemory)) return false;
            //map memory
            MappedMemoryStream mappedStream;
            if (VulkanAPI.vkMapMemory(device, stagingBufferMemory, 0,
                                       bufferSize, 0, out mappedStream)
                != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "failed to map memory!";
                VulkanAPI.vkDestroyBuffer(device, stagingBuffer);
                VulkanAPI.vkFreeMemory(device, stagingBufferMemory);
                stagingBuffer = null;
                stagingBufferMemory = null;
                return false;
            }
            // 无纹理或者没有开启纹理，纹理坐标强制设置为-1，-1
            if (textureBitmap == null || !bEnableTexture ||
                pipelineInfo.inputAssemblyState.topology == VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_LINE_LIST ||
                pipelineInfo.inputAssemblyState.topology == VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_LINE_STRIP ||
                pipelineInfo.inputAssemblyState.topology == VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_POINT_LIST)
            {
                //reset texCoords to -1
                for (int i = 0; i < points.Length; i++)
                {
                    points[i].texCoord.x = -1;
                    points[i].texCoord.y = -1;
                }
            }
           
            // write vertics data to mapped stream
            mappedStream.Write(toArray(points));
            VulkanAPI.vkUnmapMemory(device, stagingBufferMemory);
            //create destination buffer
            if (!createBuffer(bufferSize,
                      VkBufferUsageFlags.VK_BUFFER_USAGE_TRANSFER_DST_BIT |
                      VkBufferUsageFlags.VK_BUFFER_USAGE_VERTEX_BUFFER_BIT,
                      VkMemoryPropertyFlags.VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT,
                      out buffer, out memory))
            {
                errMessage = "failed to create buffer.";

                VulkanAPI.vkDestroyBuffer(device, stagingBuffer);
                VulkanAPI.vkFreeMemory(device, stagingBufferMemory);
                stagingBuffer = null;
                stagingBufferMemory = null;

                return false;
            }

            if (!copyBuffer(stagingBuffer, buffer, bufferSize))
            {
                errMessage = "failed to copy buffer.";

                VulkanAPI.vkDestroyBuffer(device, stagingBuffer);
                VulkanAPI.vkFreeMemory(device, stagingBufferMemory);
                stagingBuffer = null;
                stagingBufferMemory = null;
                VulkanAPI.vkDestroyBuffer(device, buffer);
                VulkanAPI.vkFreeMemory(device, memory);
                buffer = null;
                memory = null;
                return false;
            }

            VulkanAPI.vkDestroyBuffer(device, stagingBuffer);
            VulkanAPI.vkFreeMemory(device, stagingBufferMemory);
            stagingBuffer = null;
            stagingBufferMemory = null;
            return true;
        }        
        private bool createIndexBuffer(int[] indices, out VkBuffer buffer,out VkDeviceMemory memory)
        {
            buffer = null;
            memory = null;
            if (indices.Length < 1)
            {
                errMessage = "invalid vertic indices array!";
                return false;
            }
            VkDeviceSize bufferSize = sizeof(int) * indices.Length;
            VkBuffer stagingBuffer;
            VkDeviceMemory stagingBufferMemory;
            //create source buffer
            if (!createBuffer(bufferSize,
                          VkBufferUsageFlags.VK_BUFFER_USAGE_TRANSFER_SRC_BIT,
                          VkMemoryPropertyFlags.VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT |
                          VkMemoryPropertyFlags.VK_MEMORY_PROPERTY_HOST_COHERENT_BIT,
                          out stagingBuffer,
                          out stagingBufferMemory))
            {
                errMessage = "failed to create index buffer.";
                return false;
            }
            //map memory
            MappedMemoryStream mappedStream;
            if (VulkanAPI.vkMapMemory(device, stagingBufferMemory, 0,
                                       bufferSize, 0, out mappedStream)
                != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "failed to map memory!";
                VulkanAPI.vkDestroyBuffer(device, stagingBuffer);
                VulkanAPI.vkFreeMemory(device, stagingBufferMemory);
                stagingBuffer = null;
                stagingBufferMemory = null;
                return false;
            }
            // write vertics data to mapped stream
            mappedStream.Write(indices);
            VulkanAPI.vkUnmapMemory(device, stagingBufferMemory);
            //create destination buffer
            if (!createBuffer(bufferSize,
                      VkBufferUsageFlags.VK_BUFFER_USAGE_TRANSFER_DST_BIT |
                      VkBufferUsageFlags.VK_BUFFER_USAGE_INDEX_BUFFER_BIT,
                      VkMemoryPropertyFlags.VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT,
                      out buffer, out memory))
            {
                errMessage = "failed to map memory!";
                VulkanAPI.vkDestroyBuffer(device, stagingBuffer);
                VulkanAPI.vkFreeMemory(device, stagingBufferMemory);
                stagingBuffer = null;
                stagingBufferMemory = null;

                return false;
            }

            if (!copyBuffer(stagingBuffer, buffer, bufferSize))
            {
                errMessage = "failed to map memory!";
                VulkanAPI.vkDestroyBuffer(device, stagingBuffer);
                VulkanAPI.vkFreeMemory(device, stagingBufferMemory);
                stagingBuffer = null;
                stagingBufferMemory = null;

                VulkanAPI.vkDestroyBuffer(device, indexBuffer);
                VulkanAPI.vkFreeMemory(device, memory);
                indexBuffer = null;
                indexBufferMemory = null;
                return false;
            }

            VulkanAPI.vkDestroyBuffer(device, stagingBuffer);
            VulkanAPI.vkFreeMemory(device, stagingBufferMemory);
            stagingBuffer = null;
            stagingBufferMemory = null;
            return true;
        }
        private bool createUniformBuffer()
        {
            VkDeviceSize bufferSize = UniformBufferObject.GetSize();
            return createBuffer(bufferSize,
                          VkBufferUsageFlags.VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT,
                          VkMemoryPropertyFlags.VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT |
                          VkMemoryPropertyFlags.VK_MEMORY_PROPERTY_HOST_COHERENT_BIT,
                          out uniformBuffer, out uniformBufferMemory);
        }
        private VkFormat findSupportedFormat(VkFormat[] candidates, VkImageTiling tiling, VkFormatFeatureFlags features)
        {
            for (int i = 0; i < candidates.Length; i++)
            {
                VkFormat format = candidates[i];
                VkFormatProperties props;
                VulkanAPI.vkGetPhysicalDeviceFormatProperties(physicalDevice, format, out props);
                if (tiling == VkImageTiling.VK_IMAGE_TILING_LINEAR &&
                     (props.linearTilingFeatures & features) == features)
                {
                    return format;
                }
                else if (tiling == VkImageTiling.VK_IMAGE_TILING_OPTIMAL &&
                          (props.optimalTilingFeatures & features) == features)
                {
                    return format;
                }
            }
            errMessage = "failed to find supported format!";
            return VkFormat.VK_FORMAT_D32_SFLOAT;
        }
        private VkFormat findDepthFormat()
        {
            VkFormat[] candidates = new[]{ VkFormat.VK_FORMAT_D32_SFLOAT,
                                            VkFormat.VK_FORMAT_D32_SFLOAT_S8_UINT,
                                            VkFormat.VK_FORMAT_D24_UNORM_S8_UINT  };
            return findSupportedFormat(candidates,
                                         VkImageTiling.VK_IMAGE_TILING_OPTIMAL,
                                         VkFormatFeatureFlags.VK_FORMAT_FEATURE_DEPTH_STENCIL_ATTACHMENT_BIT);
        }
        bool hasStencilComponent(VkFormat format)
        {
            return format == VkFormat.VK_FORMAT_D32_SFLOAT_S8_UINT ||
                   format == VkFormat.VK_FORMAT_D24_UNORM_S8_UINT;
        }
        private bool createDepthResources()
        {
            VkFormat depthFormat = findDepthFormat();
            if (!createImage((int)swapChainExtent.width, (int)swapChainExtent.height,
                         depthFormat, VkImageTiling.VK_IMAGE_TILING_OPTIMAL,
                         VkImageUsageFlags.VK_IMAGE_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT,
                         VkMemoryPropertyFlags.VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT,
                         out depthImage, out depthImageMemory)) return false;
            depthImageView = createImageView(depthImage, depthFormat, VkImageAspectFlags.VK_IMAGE_ASPECT_DEPTH_BIT);
            if (!transitionImageLayout(depthImage, depthFormat,
                                    VkImageLayout.VK_IMAGE_LAYOUT_UNDEFINED,
                                    VkImageLayout.VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL)) return false;
            return true;
        }

        public Bitmap LoadTextureFrom(String fileName)
        {            
            MyTexture.forcePowerOfTwo = false;
            MyTexture.maxTextureSize = GetMax2DTextureImageSize();
            Bitmap bmp = null;
            FileInfo file = new FileInfo(fileName);
            if (file.Exists == false)
            {
                errMessage = "file ' " + fileName + " ' not exist.";
                return bmp;
            }
            try
            {
                if (file.Extension.ToUpper() == ".TGA")
                {
                    ///http://blog.csdn.net/zgke/article/details/4667499
                    //ImageTGA tga = new ImageTGA(fileName);
                    //image = tga.Image;
                }
                else
                {
                    bmp = new Bitmap(fileName);                    
                }
            }
            catch (Exception ex)
            {
                errMessage = "loading file ' " + fileName + " ' failed.";
                errMessage += "\nexception:" + ex.Message;
                return null;
            }
            return bmp;
        }

        public override Bitmap LoadTexture(String fileName, bool flip = true,bool forceoftwo = false)
        {
            return base.LoadTexture(fileName,flip,forceoftwo);
        }

        public override int BindTexture(Bitmap bmp, DataCollection.TextureMagFilter mode = DataCollection.TextureMagFilter.GL_LINEAR)
        {
            ClearTextureImageContext();
            
            textureBitmap = bmp;
            textureMode = mode;
            if (CreateTextureImageContext()) return 1;
            else return 0;
            //MyTexture.maxTextureSize = GetMax2DTextureImageSize();
            //textureBitmap = MyTexture.CreateCompitableBitmap(bmp);
            //if(flip)textureBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            //textureBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            //textureBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
            //textureBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
        }        

        public int GetBitNumFromColorFormat(VkFormat format)
        {
            int bitnum = 3;
            InverseBitmap = false;
            switch (format)
            {
                case VkFormat.VK_FORMAT_R8_UNORM:
                case VkFormat.VK_FORMAT_R8_SNORM:
                case VkFormat.VK_FORMAT_R8_USCALED:
                case VkFormat.VK_FORMAT_R8_SSCALED:
                case VkFormat.VK_FORMAT_R8_UINT:
                case VkFormat.VK_FORMAT_R8_SINT:
                case VkFormat.VK_FORMAT_R8_SRGB:
                    bitnum = 1;
                    break;
                case VkFormat.VK_FORMAT_R8G8_UNORM:
                case VkFormat.VK_FORMAT_R8G8_SNORM:
                case VkFormat.VK_FORMAT_R8G8_USCALED:
                case VkFormat.VK_FORMAT_R8G8_SSCALED:
                case VkFormat.VK_FORMAT_R8G8_UINT:
                case VkFormat.VK_FORMAT_R8G8_SINT:
                case VkFormat.VK_FORMAT_R8G8_SRGB:
                    bitnum = 2;
                    InverseBitmap = true;
                    break;
                case VkFormat.VK_FORMAT_R8G8B8_UNORM:
                case VkFormat.VK_FORMAT_R8G8B8_SNORM:
                case VkFormat.VK_FORMAT_R8G8B8_USCALED:
                case VkFormat.VK_FORMAT_R8G8B8_SSCALED:
                case VkFormat.VK_FORMAT_R8G8B8_UINT:
                case VkFormat.VK_FORMAT_R8G8B8_SINT:
                case VkFormat.VK_FORMAT_R8G8B8_SRGB:
                    bitnum = 3;
                    InverseBitmap = true;
                    break;
                case VkFormat.VK_FORMAT_B8G8R8_UNORM:
                case VkFormat.VK_FORMAT_B8G8R8_SNORM:
                case VkFormat.VK_FORMAT_B8G8R8_USCALED:
                case VkFormat.VK_FORMAT_B8G8R8_SSCALED:
                case VkFormat.VK_FORMAT_B8G8R8_UINT:
                case VkFormat.VK_FORMAT_B8G8R8_SINT:
                case VkFormat.VK_FORMAT_B8G8R8_SRGB:
                    bitnum = -3;
                    InverseBitmap = false;
                    break;
                case VkFormat.VK_FORMAT_R8G8B8A8_UNORM:
                case VkFormat.VK_FORMAT_R8G8B8A8_SNORM:
                case VkFormat.VK_FORMAT_R8G8B8A8_USCALED:
                case VkFormat.VK_FORMAT_R8G8B8A8_SSCALED:
                case VkFormat.VK_FORMAT_R8G8B8A8_UINT:
                case VkFormat.VK_FORMAT_R8G8B8A8_SINT:
                case VkFormat.VK_FORMAT_R8G8B8A8_SRGB:
                    bitnum = 4;
                    InverseBitmap = true;
                    break;
                case VkFormat.VK_FORMAT_B8G8R8A8_UNORM:
                case VkFormat.VK_FORMAT_B8G8R8A8_SNORM:
                case VkFormat.VK_FORMAT_B8G8R8A8_USCALED:
                case VkFormat.VK_FORMAT_B8G8R8A8_SSCALED:
                case VkFormat.VK_FORMAT_B8G8R8A8_UINT:
                case VkFormat.VK_FORMAT_B8G8R8A8_SINT:
                case VkFormat.VK_FORMAT_B8G8R8A8_SRGB:
                    bitnum = -4;
                    InverseBitmap = false;
                    break;
                case VkFormat.VK_FORMAT_A8B8G8R8_UNORM_PACK32:
                case VkFormat.VK_FORMAT_A8B8G8R8_SNORM_PACK32:
                case VkFormat.VK_FORMAT_A8B8G8R8_USCALED_PACK32:
                case VkFormat.VK_FORMAT_A8B8G8R8_SSCALED_PACK32:
                case VkFormat.VK_FORMAT_A8B8G8R8_UINT_PACK32:
                case VkFormat.VK_FORMAT_A8B8G8R8_SINT_PACK32:
                case VkFormat.VK_FORMAT_A8B8G8R8_SRGB_PACK32:
                    bitnum = -4;
                    break;
            }
            return bitnum;
        }       

        
        
        static public VkFormat GetMatchedFormat(Bitmap bmp)
        {
            if (bmp == null) return VkFormat.VK_FORMAT_R8G8B8_UNORM;
            VkFormat format = VkFormat.VK_FORMAT_R8G8B8_UNORM;

            switch (bmp.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    format = VkFormat.VK_FORMAT_R8G8B8_UNORM;
                    break;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb://32 位；alpha、红色、绿色和蓝色分量各使用 8 位。根据 alpha 分量，对红色、绿色和蓝色分量进行自左乘。
                    format = VkFormat.VK_FORMAT_R8G8B8A8_UNORM;
                    break;
                case PixelFormat.Format32bppRgb://红色、绿色和蓝色分量各使用 8 位。剩余的 8 位未使用。
                    format = VkFormat.VK_FORMAT_R8G8B8A8_UNORM;
                    break;
            }
            return format;
        }
        static public PixelFormat GetMatchedFormat(VkFormat format)
        {
            switch (format)
            {
                case VkFormat.VK_FORMAT_R8_UNORM:
                case VkFormat.VK_FORMAT_R8_SNORM:
                case VkFormat.VK_FORMAT_R8_USCALED:
                case VkFormat.VK_FORMAT_R8_SSCALED:
                case VkFormat.VK_FORMAT_R8_UINT:
                case VkFormat.VK_FORMAT_R8_SINT:
                case VkFormat.VK_FORMAT_R8_SRGB:
                    return PixelFormat.Format8bppIndexed;
                case VkFormat.VK_FORMAT_R8G8_UNORM:
                case VkFormat.VK_FORMAT_R8G8_SNORM:
                case VkFormat.VK_FORMAT_R8G8_USCALED:
                case VkFormat.VK_FORMAT_R8G8_SSCALED:
                case VkFormat.VK_FORMAT_R8G8_UINT:
                case VkFormat.VK_FORMAT_R8G8_SINT:
                case VkFormat.VK_FORMAT_R8G8_SRGB:
                    return PixelFormat.Format16bppArgb1555;
                case VkFormat.VK_FORMAT_R8G8B8_UNORM:
                case VkFormat.VK_FORMAT_R8G8B8_SNORM:
                case VkFormat.VK_FORMAT_R8G8B8_USCALED:
                case VkFormat.VK_FORMAT_R8G8B8_SSCALED:
                case VkFormat.VK_FORMAT_R8G8B8_UINT:
                case VkFormat.VK_FORMAT_R8G8B8_SINT:
                case VkFormat.VK_FORMAT_R8G8B8_SRGB:
                    return PixelFormat.Format24bppRgb;

                case VkFormat.VK_FORMAT_B8G8R8_UNORM:
                case VkFormat.VK_FORMAT_B8G8R8_SNORM:
                case VkFormat.VK_FORMAT_B8G8R8_USCALED:
                case VkFormat.VK_FORMAT_B8G8R8_SSCALED:
                case VkFormat.VK_FORMAT_B8G8R8_UINT:
                case VkFormat.VK_FORMAT_B8G8R8_SINT:
                case VkFormat.VK_FORMAT_B8G8R8_SRGB:
                    return PixelFormat.Format24bppRgb;

                case VkFormat.VK_FORMAT_R8G8B8A8_UNORM:
                case VkFormat.VK_FORMAT_R8G8B8A8_SNORM:
                case VkFormat.VK_FORMAT_R8G8B8A8_USCALED:
                case VkFormat.VK_FORMAT_R8G8B8A8_SSCALED:
                case VkFormat.VK_FORMAT_R8G8B8A8_UINT:
                case VkFormat.VK_FORMAT_R8G8B8A8_SINT:
                case VkFormat.VK_FORMAT_R8G8B8A8_SRGB:
                    return PixelFormat.Format32bppRgb;

                case VkFormat.VK_FORMAT_B8G8R8A8_UNORM:
                case VkFormat.VK_FORMAT_B8G8R8A8_SNORM:
                case VkFormat.VK_FORMAT_B8G8R8A8_USCALED:
                case VkFormat.VK_FORMAT_B8G8R8A8_SSCALED:
                case VkFormat.VK_FORMAT_B8G8R8A8_UINT:
                case VkFormat.VK_FORMAT_B8G8R8A8_SINT:
                case VkFormat.VK_FORMAT_B8G8R8A8_SRGB:
                    return PixelFormat.Format32bppRgb;
                case VkFormat.VK_FORMAT_A8B8G8R8_UNORM_PACK32:
                case VkFormat.VK_FORMAT_A8B8G8R8_SNORM_PACK32:
                case VkFormat.VK_FORMAT_A8B8G8R8_USCALED_PACK32:
                case VkFormat.VK_FORMAT_A8B8G8R8_SSCALED_PACK32:
                case VkFormat.VK_FORMAT_A8B8G8R8_UINT_PACK32:
                case VkFormat.VK_FORMAT_A8B8G8R8_SINT_PACK32:
                case VkFormat.VK_FORMAT_A8B8G8R8_SRGB_PACK32:
                    return PixelFormat.Format32bppRgb;
            }
            return PixelFormat.Format24bppRgb;
        }
        private VkFormat ChooseSuitableFormat()
        {
            VkFormat[] formats = new VkFormat[]
            {
                //优先选择的两种模式
                VkFormat.VK_FORMAT_B8G8R8A8_UNORM,//对应于RGBA图片序列
                VkFormat.VK_FORMAT_R8G8B8A8_UNORM,//对应于GBRA图片序列                                

                VkFormat.VK_FORMAT_R8G8B8_UNORM,
                VkFormat.VK_FORMAT_R8G8B8_SNORM,
                VkFormat.VK_FORMAT_R8G8B8_USCALED,
                VkFormat.VK_FORMAT_R8G8B8_SSCALED,
                VkFormat.VK_FORMAT_R8G8B8_UINT,
                VkFormat.VK_FORMAT_R8G8B8_SINT,
                VkFormat.VK_FORMAT_R8G8B8_SRGB,
                VkFormat.VK_FORMAT_B8G8R8_UNORM,
                VkFormat.VK_FORMAT_B8G8R8_SNORM,
                VkFormat.VK_FORMAT_B8G8R8_USCALED,
                VkFormat.VK_FORMAT_B8G8R8_SSCALED,
                VkFormat.VK_FORMAT_B8G8R8_UINT,
                VkFormat.VK_FORMAT_B8G8R8_SINT,
                VkFormat.VK_FORMAT_B8G8R8_SRGB,
               
                VkFormat.VK_FORMAT_R8G8B8A8_SNORM,
                VkFormat.VK_FORMAT_R8G8B8A8_USCALED,
                VkFormat.VK_FORMAT_R8G8B8A8_SSCALED,
                VkFormat.VK_FORMAT_R8G8B8A8_UINT,
                VkFormat.VK_FORMAT_R8G8B8A8_SINT,
                VkFormat.VK_FORMAT_R8G8B8A8_SRGB,
                
                VkFormat.VK_FORMAT_B8G8R8A8_SNORM,
                VkFormat.VK_FORMAT_B8G8R8A8_USCALED,
                VkFormat.VK_FORMAT_B8G8R8A8_SSCALED,
                VkFormat.VK_FORMAT_B8G8R8A8_UINT,
                VkFormat.VK_FORMAT_B8G8R8A8_SINT,
                VkFormat.VK_FORMAT_B8G8R8A8_SRGB,
            };
            foreach (VkFormat format in formats)
            {
                if (IsSupportFormat(format))
                    return format;
            }
            return VkFormat.VK_FORMAT_R8G8B8_UNORM;
        }

        private bool IsSupportFormat(VkFormat format)
        {
            VkFormatProperties pre;
            VulkanAPI.vkGetPhysicalDeviceFormatProperties(physicalDevice, format, out pre);
            if ((uint)pre.optimalTilingFeatures < 1) return false;
            else return true;
        }
        public bool createDefaultTextureImage()
        {
            textureBitmapFormat = ChooseSuitableFormat();
            if (!IsSupportFormat(textureBitmapFormat))
            {
                errMessage = "texture bitmap format not supported.";
                return false;
            }

            //get the bitmap Bytes Number 3(24),4(32)
            int bitNum = GetBitNumFromColorFormat(textureBitmapFormat);
            if (bitNum < 0) bitNum = bitNum * -1;

            //defalut texture image, 1 pixel ,white color
            byte[] images = new byte[bitNum];
            for (int i = 0; i < bitNum; i++) images[i] = 255;

            int texWidth = 1;
            int texHeight = 1;
            VkDeviceSize imageSize = texWidth * texHeight * bitNum;

            VkBuffer stagingBuffer;
            VkDeviceMemory stagingBufferMemory;
            createBuffer(imageSize, VkBufferUsageFlags.VK_BUFFER_USAGE_TRANSFER_SRC_BIT,
                          VkMemoryPropertyFlags.VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT |
                          VkMemoryPropertyFlags.VK_MEMORY_PROPERTY_HOST_COHERENT_BIT,
                          out stagingBuffer, out stagingBufferMemory);
            MappedMemoryStream data;
            if (VulkanAPI.vkMapMemory(device, stagingBufferMemory, 0, imageSize, 0, out data)
                != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "failed to map memory!";
                return false;
            }
            data.Write(images);
            VulkanAPI.vkUnmapMemory(device, stagingBufferMemory);
            if (!createImage(texWidth, texHeight, textureBitmapFormat,
                        VkImageTiling.VK_IMAGE_TILING_OPTIMAL,
                        VkImageUsageFlags.VK_IMAGE_USAGE_TRANSFER_DST_BIT |
                        VkImageUsageFlags.VK_IMAGE_USAGE_SAMPLED_BIT,
                         VkMemoryPropertyFlags.VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT,
                         out textureImage, out textureImageMemory)) return false;
            if (!transitionImageLayout(textureImage, textureBitmapFormat, VkImageLayout.VK_IMAGE_LAYOUT_UNDEFINED,
                                  VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL)) return false;
            copyBufferToImage(stagingBuffer, textureImage, texWidth, texHeight);
            if (!transitionImageLayout(textureImage, textureBitmapFormat,
                                  VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
                                  VkImageLayout.VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL)) return false;
            VulkanAPI.vkDestroyBuffer(device, stagingBuffer);
            VulkanAPI.vkFreeMemory(device, stagingBufferMemory);
            return true;
        }
        public VkFormat GetVKFormat(Bitmap bmp)
        {
            if (bmp == null) return VkFormat.VK_FORMAT_R8G8B8_UNORM;
            VkFormat format = VkFormat.VK_FORMAT_R8G8B8_UNORM;

            switch (bmp.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    format = VkFormat.VK_FORMAT_R8G8B8_UNORM;
                    break;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb://32 位；alpha、红色、绿色和蓝色分量各使用 8 位。根据 alpha 分量，对红色、绿色和蓝色分量进行自左乘。
                    format = VkFormat.VK_FORMAT_R8G8B8A8_UNORM;
                    break;
                case PixelFormat.Format32bppRgb://红色、绿色和蓝色分量各使用 8 位。剩余的 8 位未使用。
                    format = VkFormat.VK_FORMAT_R8G8B8A8_UNORM;
                    break;
            }
            return format;
        }
        private bool createTextureImage(out VkImage image,out VkDeviceMemory memory)
        {
            image = null;
            memory = null;
            if (textureBitmap == null)
            {
                errMessage = "Texture bitmap is empty, please 'LoadTexture' first.";
                return false;
            }
            int texWidth = textureBitmap.Width;
            int texHeight = textureBitmap.Height;
            int bitNum = GetBitNumFromColorFormat(textureBitmapFormat);

            VkDeviceSize imageSize = texWidth * texHeight * Math.Abs(bitNum);

            VkBuffer stagingBuffer;
            VkDeviceMemory stagingBufferMemory;
            if (!createBuffer(imageSize, VkBufferUsageFlags.VK_BUFFER_USAGE_TRANSFER_SRC_BIT,
                          VkMemoryPropertyFlags.VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT |
                          VkMemoryPropertyFlags.VK_MEMORY_PROPERTY_HOST_COHERENT_BIT,
                          out stagingBuffer, out stagingBufferMemory))
            {
                errMessage = "faild to create texture image:" + errMessage;
                return false;
            }

            MappedMemoryStream data;
            if (VulkanAPI.vkMapMemory(device, stagingBufferMemory, 0, imageSize, 0, out data)
                != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "failed to map memory!";
                return false;
            }
            // convert bitmap to byte[], then write to mapped memory
            data.Write(BitmapToIntBytes(textureBitmap, bitNum,false));
            VulkanAPI.vkUnmapMemory(device, stagingBufferMemory);

            if (!createImage(texWidth, texHeight, textureBitmapFormat,
                        VkImageTiling.VK_IMAGE_TILING_OPTIMAL,
                        VkImageUsageFlags.VK_IMAGE_USAGE_TRANSFER_DST_BIT |
                        VkImageUsageFlags.VK_IMAGE_USAGE_SAMPLED_BIT,
                         VkMemoryPropertyFlags.VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT,
                         out image, out memory)) return false;

            if (!transitionImageLayout(textureImage, textureBitmapFormat, VkImageLayout.VK_IMAGE_LAYOUT_UNDEFINED,
                                  VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL)) return false;

            copyBufferToImage(stagingBuffer, image, texWidth, texHeight);

            if (!transitionImageLayout(image, textureBitmapFormat,
                                  VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
                                  VkImageLayout.VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL)) return false;
            
            VulkanAPI.vkDestroyBuffer(device, stagingBuffer);
            VulkanAPI.vkFreeMemory(device, stagingBufferMemory);

            return true;
        }

        private bool transitionImageLayout(VkImage image, VkFormat format,
                                VkImageLayout oldLayout, VkImageLayout newLayout)
        {
            VkCommandBuffer commandBuffer = beginSingleTimeCommands();

            VkImageMemoryBarrier barrier = new VkImageMemoryBarrier();
            barrier.oldLayout = oldLayout;
            barrier.newLayout = newLayout;
            barrier.srcQueueFamilyIndex = (~0U);//VK_QUEUE_FAMILY_IGNORED;
            barrier.dstQueueFamilyIndex = (~0U);//VK_QUEUE_FAMILY_IGNORED;
            barrier.image = image;
            //barrier.subresourceRange.aspectMask = VkImageAspectFlags.VK_IMAGE_ASPECT_COLOR_BIT;
            if (newLayout == VkImageLayout.VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL)
            {
                barrier.subresourceRange.aspectMask = VkImageAspectFlags.VK_IMAGE_ASPECT_DEPTH_BIT;
                if (hasStencilComponent(format))
                    barrier.subresourceRange.aspectMask |= VkImageAspectFlags.VK_IMAGE_ASPECT_STENCIL_BIT;
            }
            else barrier.subresourceRange.aspectMask = VkImageAspectFlags.VK_IMAGE_ASPECT_COLOR_BIT;
            barrier.subresourceRange.baseMipLevel = 0;
            barrier.subresourceRange.levelCount = 1;
            barrier.subresourceRange.baseArrayLayer = 0;
            barrier.subresourceRange.layerCount = 1;
            barrier.srcAccessMask = 0; // TODO
            barrier.dstAccessMask = 0; // TODO
            VkPipelineStageFlags sourceStage;
            VkPipelineStageFlags destinationStage;
            if (oldLayout == VkImageLayout.VK_IMAGE_LAYOUT_UNDEFINED &&
                newLayout == VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL)
            {
                barrier.srcAccessMask = 0;
                barrier.dstAccessMask = VkAccessFlags.VK_ACCESS_TRANSFER_WRITE_BIT;
                sourceStage = VkPipelineStageFlags.VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;
                destinationStage = VkPipelineStageFlags.VK_PIPELINE_STAGE_TRANSFER_BIT;
            }
            else if (oldLayout == VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL &&
                     newLayout == VkImageLayout.VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL)
            {
                barrier.srcAccessMask = VkAccessFlags.VK_ACCESS_TRANSFER_WRITE_BIT;
                barrier.dstAccessMask = VkAccessFlags.VK_ACCESS_SHADER_READ_BIT;
                sourceStage = VkPipelineStageFlags.VK_PIPELINE_STAGE_TRANSFER_BIT;
                destinationStage = VkPipelineStageFlags.VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
            }
            else if (oldLayout == VkImageLayout.VK_IMAGE_LAYOUT_UNDEFINED &&
                  newLayout == VkImageLayout.VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL)
            {
                barrier.srcAccessMask = 0;
                barrier.dstAccessMask = VkAccessFlags.VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_READ_BIT |
                                        VkAccessFlags.VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT;
                sourceStage = VkPipelineStageFlags.VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;
                destinationStage = VkPipelineStageFlags.VK_PIPELINE_STAGE_EARLY_FRAGMENT_TESTS_BIT;
            }
            else
            {
                errMessage = "unsupported layout transition!";
                return false;
            }
            VulkanAPI.vkCmdPipelineBarrier(commandBuffer, sourceStage, destinationStage, 0,
                                            null, null, new[] { barrier });
            endSingleTimeCommands(commandBuffer);
            return true;
        }
        private void copyBufferToImage(VkBuffer buffer, VkImage image, int _width, int _height)
        {
            VkCommandBuffer commandBuffer = beginSingleTimeCommands();
            VkBufferImageCopy region = new VkBufferImageCopy();
            region.bufferOffset = 0;
            region.bufferRowLength = 0;
            region.bufferImageHeight = 0;
            region.imageSubresource.aspectMask = VkImageAspectFlags.VK_IMAGE_ASPECT_COLOR_BIT;
            region.imageSubresource.mipLevel = 0;
            region.imageSubresource.baseArrayLayer = 0;
            region.imageSubresource.layerCount = 1;
            region.imageOffset = new VkOffset3D() { x = 0, y = 0, z = 0 };
            region.imageExtent = new VkExtent3D { width = (uint)_width, height = (uint)_height, depth = 1 };
            VulkanAPI.vkCmdCopyBufferToImage(commandBuffer, buffer, image,
                                    VkImageLayout.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL,
                                    new[] { region });
            endSingleTimeCommands(commandBuffer);
        }
        private bool createImage(int width, int height, VkFormat format, VkImageTiling tiling,
                                 VkImageUsageFlags usage, VkMemoryPropertyFlags properties,
                                 out VkImage image, out VkDeviceMemory imageMemory)
        {
            image = null;
            imageMemory = null;
            VkImageCreateInfo imageInfo = new VkImageCreateInfo();
            imageInfo.imageType = VkImageType.VK_IMAGE_TYPE_2D;
            imageInfo.extent.width = (uint)width;
            imageInfo.extent.height = (uint)height;
            imageInfo.extent.depth = 1;
            imageInfo.mipLevels = 1;
            imageInfo.arrayLayers = 1;
            imageInfo.format = format;
            imageInfo.tiling = tiling;
            imageInfo.initialLayout = VkImageLayout.VK_IMAGE_LAYOUT_UNDEFINED;
            imageInfo.usage = usage;
            imageInfo.samples = VkSampleCountFlags.VK_SAMPLE_COUNT_1_BIT;
            imageInfo.sharingMode = VkSharingMode.VK_SHARING_MODE_EXCLUSIVE;
            if (VulkanAPI.vkCreateImage(device, ref imageInfo, out image)
                != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "failed to create image!";
                return false;
            }

            VkMemoryRequirements memRequirements;
            VulkanAPI.vkGetImageMemoryRequirements(device, image, out memRequirements);

            VkMemoryAllocateInfo allocInfo = new VkMemoryAllocateInfo();
            allocInfo.allocationSize = memRequirements.size;
            allocInfo.memoryTypeIndex = (uint)findMemoryType((int)memRequirements.memoryTypeBits, properties);
            if (VulkanAPI.vkAllocateMemory(device, ref allocInfo, out imageMemory)
                != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "failed to allocate image memory!";
                return false;
            }
            VulkanAPI.vkBindImageMemory(device, image, imageMemory, 0);
            return true;
        }
        private bool createTextureImageView()
        {
            if (textureImageView != null) return true;

            textureImageView = createImageView(textureImage, textureBitmapFormat, VkImageAspectFlags.VK_IMAGE_ASPECT_COLOR_BIT);
            if (textureImageView == null) return false;
            else return true;
        }
        private bool createTextureSampler()
        {
            if (textureSampler != null) return true;

            if (VulkanAPI.vkCreateSampler(device, ref textureSamplerInfo, out textureSampler)
                 != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "failed to create texture sampler!";
                return false;
            }
            return true;
        }
        VkImageView createImageView(VkImage image, VkFormat format, VkImageAspectFlags aspectFlags)
        {
            VkImageViewCreateInfo viewInfo = new VkImageViewCreateInfo();
            viewInfo.image = image;
            viewInfo.viewType = VkImageViewType.VK_IMAGE_VIEW_TYPE_2D;
            viewInfo.format = format;
            viewInfo.subresourceRange.aspectMask = aspectFlags;
            viewInfo.subresourceRange.baseMipLevel = 0;
            viewInfo.subresourceRange.levelCount = 1;
            viewInfo.subresourceRange.baseArrayLayer = 0;
            viewInfo.subresourceRange.layerCount = 1;
            VkImageView imageView;
            //？？？？？可能会崩溃？？？？
            if (VulkanAPI.vkCreateImageView(device, ref viewInfo, out imageView)
                 != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "failed to create texture image view!";
                return null;
            }
            return imageView;
        }
        private bool createSwapChainImageViews()
        {
            swapChainImageViews = new VkImageView[swapChainImages.Length];
            for (int i = 0; i < swapChainImages.Length; i++)
            {
                swapChainImageViews[i] = createImageView(swapChainImages[i], swapChainImageFormat, VkImageAspectFlags.VK_IMAGE_ASPECT_COLOR_BIT);
                if (swapChainImageViews[i] == null) return false;
            }
            return true;
        }
        private byte[] ReadSpvFile(string path)
        {
            byte[] data = null;
            try
            {
                FileStream sFile = new FileStream(path, FileMode.Open);
                int size = (int)sFile.Length;
                data = new byte[size];
                sFile.Seek(0, SeekOrigin.Begin);
                //sFile.ReadByte();
                sFile.Read(data, 0, size);
                sFile.Close();
            }
            catch
            {
                return null;
            }
            return data;
        }
        /* 
         * https://github.com/KhronosGroup/Vulkan-LoaderAndValidationLayers/issues/706
         * When using the NVidia driver and enabling the VK_NV_glsl_shader extension, 
         * the shader validation appears to assume the shader must be SPIRV and throws this error:
           SC: SPIR-V module not valid: Invalid SPIR-V magic number. 
           Exception thrown at 0x00007FF8F9DD51E4 (VkLayer_core_validation.dll) 
           in Viewer.exe: 0xC0000005: Access violation reading location 0x000001A629FE1574.
           The extension allows GLSL shaders to be passed in instead of SPIR-V. 
           The debug layer does not allow for non-SPIRV shaders when this extension is present. 
           If the exension is enabled and the shader signature is not SPIRV,
           we should probably skip continuing with the validator.
         */
        private VkShaderModule createShaderModule(byte[] code)
        {
            VkShaderModuleCreateInfo createInfo = new VkShaderModuleCreateInfo();
            createInfo.shaderCodeBinary = code;
            VkShaderModule shaderModule = null;
            if (VulkanAPI.vkCreateShaderModule(device, ref createInfo, out shaderModule)
                != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "failed to create shader module!" + GetValidationLayerReports();
                return null;
            }

            return shaderModule;
        }
        private VkPipelineVertexInputStateCreateInfo CreateVertexInputState()
        {
            // postion description
            var attribPosition = new VkVertexInputAttributeDescription()
            {
                location = 0,
                binding = 0,
                offset = 0,
                format = VkFormat.VK_FORMAT_R32G32B32_SFLOAT,
            };
            // color description
            var attribColor = new VkVertexInputAttributeDescription()
            {
                location = 1,
                binding = 0,
                offset = 12,
                format = VkFormat.VK_FORMAT_R32G32B32A32_SFLOAT,
            };
            var vertexInputBindingDesc = new VkVertexInputBindingDescription()
            {
                binding = 0,
                inputRate = VkVertexInputRate.VK_VERTEX_INPUT_RATE_VERTEX,
                stride = (uint)Marshal.SizeOf(typeof(vec2)),
            };
            var vertexInputState = new VkPipelineVertexInputStateCreateInfo()
            {
                attributeDescriptions = new[] { attribPosition, attribColor },
                bindingDescriptions = new[] { vertexInputBindingDesc },
            };
            return vertexInputState;
        }
        private bool createFramebuffers()
        {
            swapChainFramebuffers = new VkFramebuffer[swapChainImageViews.Length];
            for (int i = 0; i < swapChainImageViews.Length; i++)
            {
                VkImageView[] attachments = new[]
                {
                    swapChainImageViews[i],depthImageView
                };
                VkFramebufferCreateInfo framebufferInfo = new VkFramebufferCreateInfo();
                framebufferInfo.renderPass = renderPass;
                framebufferInfo.attachments = attachments;
                framebufferInfo.width = swapChainExtent.width;
                framebufferInfo.height = swapChainExtent.height;
                framebufferInfo.layers = 1;
                //
                if (VulkanAPI.vkCreateFramebuffer(device, ref framebufferInfo, out swapChainFramebuffers[i])
                    != SharpVulkan.VkResult.VK_SUCCESS)
                {
                    errMessage = "failed to create framebuffer!";
                    return false;
                }
            }
            errMessage = "create framebuffers successfully";
            return true;
        }

        private bool createCommandPool()
        {
            QueueFamilyIndices queueFamilyIndices = findQueueFamilies(physicalDevice);
            VkCommandPoolCreateInfo poolInfo = new VkCommandPoolCreateInfo();
            poolInfo.queueFamilyIndex = (uint)queueFamilyIndices.graphicsFamily;
            poolInfo.flags = VkCommandPoolCreateFlags.VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT; // Optional
            if (VulkanAPI.vkCreateCommandPool(device, ref poolInfo, out commandPool)
                 != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "failed to create command pool!";
                return false;
            }
            errMessage = "created command pool successfully.";
            return true;
        }
        public void UpdateDescriptorSet(CModelDescriptor model)
        {
            if (model.descriptorSet == null) return;
            if (model.uniformBuffer == null) return;
            VkDescriptorBufferInfo bufferInfo = new VkDescriptorBufferInfo();
            bufferInfo.buffer = model.uniformBuffer;
            bufferInfo.offset = 0;
            bufferInfo.range = UniformBufferObject.GetSize();

            VkWriteDescriptorSet[] descriptorWrites;
            if (model.textureImageView != null && model.textureSampler != null)
                descriptorWrites = new VkWriteDescriptorSet[2];
            else descriptorWrites = new VkWriteDescriptorSet[1];

            descriptorWrites[0] = new VkWriteDescriptorSet();
            descriptorWrites[0].dstSet = model.descriptorSet;
            descriptorWrites[0].dstBinding = 0;
            descriptorWrites[0].dstArrayElement = 0;
            descriptorWrites[0].descriptorType = VkDescriptorType.VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
            descriptorWrites[0].descriptorCount = 1;
            descriptorWrites[0].pBufferInfo = new[] { bufferInfo };

            if (model.textureImageView != null && model.textureSampler != null )
            {
                VkDescriptorImageInfo imageInfo = new VkDescriptorImageInfo();
                imageInfo.imageLayout = VkImageLayout.VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
                imageInfo.imageView = model.textureImageView;
                imageInfo.sampler = model.textureSampler;

                descriptorWrites[1] = new VkWriteDescriptorSet();
                descriptorWrites[1].dstSet = model.descriptorSet;
                descriptorWrites[1].dstBinding = 1;
                descriptorWrites[1].dstArrayElement = 0;
                descriptorWrites[1].descriptorType = VkDescriptorType.VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER;
                descriptorWrites[1].descriptorCount = 1;
                descriptorWrites[1].pImageInfo = new[] { imageInfo };
            }
            try 
            {
                VulkanAPI.vkUpdateDescriptorSets(model.device, descriptorWrites, null);
                return ;
            }
#pragma warning disable CS0168 // 声明了变量“e”，但从未使用过
            catch(Exception e)
#pragma warning restore CS0168 // 声明了变量“e”，但从未使用过
            {
                return ;
            }
        }
       
        private bool UpdateCommand(List<CModel>models)
        {
            lock (modelLocker)
            {
                foreach(CModel obj in models)
                {
                    CModelDescriptor model = obj as CModelDescriptor;
                    if (model != null && model.Visible) 
                    {
                        model.UpdateDescriptorSet();
                        model.UpdateUniformBuffer();
                    }
                }                

                for (int i = 0; i < commandBuffers.Length; i++)
                {
                    VkCommandBufferBeginInfo beginInfo = new VkCommandBufferBeginInfo();
                    //VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT: 只能被提交一次，之后可能被重置。 
                    //VK_COMMAND_BUFFER_USAGE_RENDER_PASS_CONTINUE_BIT: 整个次command buffer 将会在reder pass中，主command buffer 将忽略此值。 
                    //VK_COMMAND_BUFFER_USAGE_SIMULTANEOUS_USE_BIT: command buffer 在等待执行时可以被重复提交,很有可能在上一个帧(frame)尚未画完，下一个帧的绘画请求就已经提交了。                
                    beginInfo.flags = VkCommandBufferUsageFlags.VK_COMMAND_BUFFER_USAGE_SIMULTANEOUS_USE_BIT;
                    beginInfo.pInheritanceInfo = IntPtr.Zero; // Optional
                    VulkanAPI.vkBeginCommandBuffer(commandBuffers[i]);
                    //Starting a render pass
                    VkRenderPassBeginInfo renderPassInfo = new VkRenderPassBeginInfo();
                    renderPassInfo.renderPass = renderPass;
                    renderPassInfo.framebuffer = swapChainFramebuffers[i];
                    renderPassInfo.renderArea.offset.x = 0;
                    renderPassInfo.renderArea.offset.y = 0;
                    renderPassInfo.renderArea.extent = swapChainExtent;

                    //clear color
                    VkClearColorValue _clearColor = new VkClearColorValue();
                    _clearColor.valF32.R = clearColor.x; //0.125f;
                    _clearColor.valF32.G = clearColor.y; //0.25f;
                    _clearColor.valF32.B = clearColor.z; //0.5f;
                    _clearColor.valF32.A = 1.0f;
                    VkClearDepthStencilValue _depthStencil = new VkClearDepthStencilValue();
                    _depthStencil.stencil = 0;
                    _depthStencil.depth = 1.0f;
                    VkClearValue[] clearValues = new VkClearValue[2];
                    clearValues[0] = new VkClearValue() { color = _clearColor };
                    clearValues[1] = new VkClearValue() { depthStencil = _depthStencil };
                    renderPassInfo.pClearValues = clearValues;

                    //begin and end rander pass                 
                    VulkanAPI.vkCmdBeginRenderPass(commandBuffers[i], ref renderPassInfo, new VkSubpassContents());

                    //UpdateUniformBuffer();
                    foreach (CModel obj in models)                                              
                    {
                        CModelDescriptor model = obj as CModelDescriptor;
                        if (!model.Visible) continue;

                        VulkanAPI.vkCmdBindPipeline(commandBuffers[i], new VkPipelineBindPoint(), model.graphicsPipeline);
                        VulkanAPI.vkCmdBindVertexBuffers(commandBuffers[i], (uint)(0), 1,
                                                             new[] { model.vertexBuffer },
                                                             new VkDeviceSize[] { 0 });
                        VulkanAPI.vkCmdBindIndexBuffer(commandBuffers[i],
                                                        model.indexBuffer, 0,
                                                        VkIndexType.VK_INDEX_TYPE_UINT32);                       

                        UpdateDynamicPipelinePara(commandBuffers[i], model);
                        model.UpdateUniformBuffer();
                        model.UpdateDescriptorSet();
                        //UpdateDescriptorSet(pModelDescriptores[k]);

                        VulkanAPI.vkCmdBindDescriptorSets(commandBuffers[i],
                                                          VkPipelineBindPoint.VK_PIPELINE_BIND_POINT_GRAPHICS,
                                                          pipelineLayout, 0,
                                                          new[] { model.descriptorSet }, null);
                       
                        
                        VulkanAPI.vkCmdDrawIndexed(commandBuffers[i], model.indicesLength, 1, (uint)(0), 0, (uint)0);
                        
                    }

                    //end command buffer
                    VulkanAPI.vkCmdEndRenderPass(commandBuffers[i]);
                    if (VulkanAPI.vkEndCommandBuffer(commandBuffers[i]) != SharpVulkan.VkResult.VK_SUCCESS)
                    {
                        errMessage = "failed to record command buffer!";
                        return false;
                    }
                }
            }
            return true;
        }
        private bool createCommandBuffers()
        {
            if (commandBuffers == null)
            {
                commandBuffers = new VkCommandBuffer[swapChainFramebuffers.Length];
                VkCommandBufferAllocateInfo allocInfo = new VkCommandBufferAllocateInfo();
                allocInfo.commandPool = commandPool;
                allocInfo.level = VkCommandBufferLevel.VK_COMMAND_BUFFER_LEVEL_PRIMARY;
                allocInfo.commandBufferCount = (uint)commandBuffers.Length;

                if (VulkanAPI.vkAllocateCommandBuffers(device, ref allocInfo, out commandBuffers)
                     != SharpVulkan.VkResult.VK_SUCCESS)
                {
                    errMessage = "failed to allocate command buffers!";
                    return false;
                }
                //UpdateCommand();
                errMessage = "Created command buffer successfully.";
            }
            return true;
        }
        private bool createSemaphores()
        {
            VkSemaphoreCreateInfo semaphoreInfo = new VkSemaphoreCreateInfo();
            semaphoreInfo.flags = (VkSemaphoreCreateFlags)0;
            if (VulkanAPI.vkCreateSemaphore(device, ref semaphoreInfo, out imageAvailableSemaphore)
                 != SharpVulkan.VkResult.VK_SUCCESS ||
                 VulkanAPI.vkCreateSemaphore(device, ref semaphoreInfo, out renderFinishedSemaphore)
                 != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "failed to create semaphores!";
                return false;
            }

            errMessage = "create semaphores successfully";
            return true;
        }

        private bool createRenderPass()
        {
            //Attachment description
            VkAttachmentDescription colorAttachment = new VkAttachmentDescription();
            colorAttachment.format = swapChainImageFormat;
            colorAttachment.samples = VkSampleCountFlags.VK_SAMPLE_COUNT_1_BIT;
            colorAttachment.loadOp = VkAttachmentLoadOp.VK_ATTACHMENT_LOAD_OP_CLEAR;
            colorAttachment.storeOp = VkAttachmentStoreOp.VK_ATTACHMENT_STORE_OP_STORE;
            colorAttachment.stencilLoadOp = VkAttachmentLoadOp.VK_ATTACHMENT_LOAD_OP_DONT_CARE;
            colorAttachment.stencilStoreOp = VkAttachmentStoreOp.VK_ATTACHMENT_STORE_OP_DONT_CARE;
            colorAttachment.initialLayout = VkImageLayout.VK_IMAGE_LAYOUT_UNDEFINED;
            colorAttachment.finalLayout = VkImageLayout.VK_IMAGE_LAYOUT_PRESENT_SRC_KHR;

            //Subpasses and attachment references
            VkAttachmentReference colorAttachmentRef = new VkAttachmentReference();
            colorAttachmentRef.attachment = 0;
            colorAttachmentRef.layout = VkImageLayout.VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;

            VkAttachmentDescription depthAttachment = new VkAttachmentDescription();
            depthAttachment.format = findDepthFormat();
            depthAttachment.samples = VkSampleCountFlags.VK_SAMPLE_COUNT_1_BIT;
            depthAttachment.loadOp = VkAttachmentLoadOp.VK_ATTACHMENT_LOAD_OP_CLEAR;
            depthAttachment.storeOp = VkAttachmentStoreOp.VK_ATTACHMENT_STORE_OP_DONT_CARE;
            depthAttachment.stencilLoadOp = VkAttachmentLoadOp.VK_ATTACHMENT_LOAD_OP_DONT_CARE;
            depthAttachment.stencilStoreOp = VkAttachmentStoreOp.VK_ATTACHMENT_STORE_OP_DONT_CARE;
            depthAttachment.initialLayout = VkImageLayout.VK_IMAGE_LAYOUT_UNDEFINED;
            depthAttachment.finalLayout = VkImageLayout.VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;

            VkAttachmentReference depthAttachmentRef = new VkAttachmentReference();
            depthAttachmentRef.attachment = 1;
            depthAttachmentRef.layout = VkImageLayout.VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;

            VkSubpassDescription subpass = new VkSubpassDescription();
            subpass.pipelineBindPoint = VkPipelineBindPoint.VK_PIPELINE_BIND_POINT_GRAPHICS;
            subpass.colorAttachments = new[] { colorAttachmentRef };
            subpass.depthStencilAttachment = depthAttachmentRef;
            //SubpassDependency
            VkSubpassDependency dependency = new VkSubpassDependency();
            dependency.srcSubpass = (~0U); //VK_SUBPASS_EXTERNAL (~0U) 4294967295
            dependency.dstSubpass = 0;
            dependency.srcStageMask = VkPipelineStageFlags.VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
            dependency.srcAccessMask = 0;
            dependency.dstStageMask = VkPipelineStageFlags.VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
            dependency.dstAccessMask = VkAccessFlags.VK_ACCESS_COLOR_ATTACHMENT_READ_BIT |
                                       VkAccessFlags.VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
            VkSubpassDependency[] dependencies = new VkSubpassDependency[] { dependency };
            //Create Render Pass
            VkRenderPassCreateInfo renderPassInfo = new VkRenderPassCreateInfo();
            renderPassInfo.attachments = new[] { colorAttachment, depthAttachment };
            renderPassInfo.subpasses = new[] { subpass };
            renderPassInfo.dependencies = dependencies;

            if (VulkanAPI.vkCreateRenderPass(device, ref renderPassInfo, out renderPass)
                 != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "failed to create render pass!";
                return false;
            }

            errMessage = "created render pass sucessfully!";
            return true;
        }
        private bool createLogicalDevice()
        {
            QueueFamilyIndices indices = findQueueFamilies(physicalDevice);

            int[] uniqueQueueFamilies;
            if (indices.graphicsFamily == indices.presentFamily)
                uniqueQueueFamilies = new int[] { indices.graphicsFamily };
            else
                uniqueQueueFamilies = new int[] { indices.graphicsFamily, indices.presentFamily };
            VkDeviceQueueCreateInfo[] queueCreateInfos = new VkDeviceQueueCreateInfo[uniqueQueueFamilies.Length];
            float[] queuePriority = new float[] { 1.0f };
            for (int i = 0; i < uniqueQueueFamilies.Length; i++)
            {
                VkDeviceQueueCreateInfo queueCreateInfo = new VkDeviceQueueCreateInfo();
                queueCreateInfo.queueFamilyIndex = (uint)uniqueQueueFamilies[i];
                queueCreateInfo.queuePriorities = queuePriority;
                queueCreateInfos[i] = queueCreateInfo;
            }
            VkPhysicalDeviceFeatures deviceFeatures = new VkPhysicalDeviceFeatures();
            VkDeviceCreateInfo createInfo = new VkDeviceCreateInfo();
            createInfo.QueueCreateInfos = queueCreateInfos;
            createInfo.EnabledFeatures = deviceFeatures;
            createInfo.EnabledExtensionNames = deviceExtensions;
            createInfo.EnabledLayerNames = null;
            if (enableValidationLayers)
            {
#pragma warning disable CS0162 // 检测到无法访问的代码
                createInfo.EnabledLayerNames = validationLayers;
#pragma warning restore CS0162 // 检测到无法访问的代码
            }
            if (VulkanAPI.vkCreateDevice(physicalDevice, ref createInfo, out device) != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "create vulkan device faild.";
                return false;
            }
            errMessage = "created vulkan device sucessfully!";

            VulkanAPI.vkGetDeviceQueue(device, (uint)indices.graphicsFamily, 0, out graphicsQueue);
            VulkanAPI.vkGetDeviceQueue(device, (uint)indices.presentFamily, 0, out presentQueue);
            return true;
        }
        public override void onWindowResized(int width, int height)
        {
            if (width == 0 || height == 0) return;
            windowWidth = width;
            windowHeight = height;
            ResetViewport();
            recreateSwapChain();
        }
        public override bool Initialize(IntPtr window, string title = "vulkan", int width = 800, int height = 600)
        {
            try 
            {
                glfw3.Glfw.Init();

                windowHandle = window;
                windowWidth = width;
                windowHeight = height;
                windowTitle = title;
                string vertspv = graphicLIBPath + "\\" + "vert.spv";
                string fragspv = graphicLIBPath + "\\" + "frag.spv";

                if (!CreateVKInstance()) return false;
                if (!setupDebugCallback()) return false;
                if (!createSurface()) return false;                
                if (!pickPhysicalDevice()) return false;                
                if (!createLogicalDevice()) return false;
                if (!createSwapChain()) return false;
                if (!createSwapChainImageViews()) return false;
                if (!createRenderPass()) return false;

                //create default for graphicpipeline
                //if (!LoadShader(@"..\..\Shaders\defaultvert.spv", @"..\..\Shaders\defaultfrag.spv")) return false;
                //if (!LoadShader(@"..\..\Shaders\vert.spv", @"..\..\Shaders\frag.spv")) return false;
                if (!LoadShader(vertspv, fragspv)) return false;

                if (!createDescriptorSetLayout()) return false;
                //if (!CreateGraphicsPipeLine(null)) return false;
                if (!CreateDefaultGraphicsPipeLine()) return false;
                if (!createCommandPool()) return false;
                if (!createDepthResources()) return false;
                if (!createFramebuffers()) return false;

                if (!createDefaultTextureImage()) return false;
                //if (!createTextureImage()) return false;
                if (!createTextureImageView()) return false;

                CreateDefaultTextureSamplerInfo();
                if (!createTextureSampler()) return false;

                //if (!createVertexBuffer()) return false;
                //if (!createIndexBuffer()) return false;
                //if (!createUniformBuffer()) return false;
                if (!createDescriptorPool()) return false;
                if (!createDescriptorSet()) return false;
                if (!createCommandBuffers()) return false;
                if (!createSemaphores()) return false;

                initialized = true;
                engine = gEngine.vulkan;
                return true;
            }
            catch(Exception ex)
            {
                errMessage = ex.Message;
                return false;
            }
        }
        static Semaphore sema = new Semaphore(5, 5);

        private void recreateSwapChain()
        {
            m_Semaphore.WaitOne();
            lock (modelLocker)
            {
                VulkanAPI.vkDeviceWaitIdle(device);
                cleanupSwapChain();

                createSwapChain();
                createSwapChainImageViews();
                createRenderPass();

                createDepthResources();
                createFramebuffers();
                createCommandBuffers();
                //UpdateCommand();
            }
            m_Semaphore.Release();
        }
        private void CopyMemoryTo(UniformBufferObject obj, IntPtr data)
        {
            float[] p1 = obj.view.to_array();
            float[] p2 = obj.model.to_array();
            float[] p3 = obj.proj.to_array();
            int offset = 0;
            Marshal.Copy(p1, 0, data + offset, p1.Length);
            offset += p1.Length;
            Marshal.Copy(p2, 0, data + offset, p2.Length);
            offset += p2.Length;
            Marshal.Copy(p3, 0, data + offset, p3.Length);
            return;

#pragma warning disable CS0162 // 检测到无法访问的代码
            int size = UniformBufferObject.GetSize() / 3;
#pragma warning restore CS0162 // 检测到无法访问的代码
            byte[] bytes = new byte[size];


            IntPtr buffer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(obj.model, buffer, false);
            Marshal.Copy(buffer, bytes, 0, size);
            Marshal.Copy(bytes, 0, data + offset, size);
            offset += size;

            Marshal.StructureToPtr(obj.view, buffer, false);
            Marshal.Copy(buffer, bytes, 0, size);
            Marshal.Copy(bytes, 0, data + offset, size);
            offset += size;

            Marshal.StructureToPtr(obj.proj, buffer, false);
            Marshal.Copy(buffer, bytes, 0, size);
            Marshal.Copy(bytes, 0, data + offset, size);

            bytes = null;
        }
        /// <summary>
        /// 重设所有模型得统一参数，矩阵，光照，视点，材质等
        /// </summary>
        public override void ResetModelUniformMatrixs(List<CModel> models)
        {
            lock (modelLocker)
            {
                foreach (CModel obj in models)
                {
                    CModelDescriptor model = obj as CModelDescriptor;
                    //update model view eyepos,modelview
                    model.m_modelMatrix.proj = m_modelMatrix.proj;
                    model.m_modelMatrix.view = m_modelMatrix.view;

                    // this model include global rotate
                    if (model.m_enableRotate) model.m_modelMatrix.model = m_modelMatrix.model;

                    model.m_modelMatrix.eyepos = new vec4(m_eye, 1.0f);
                    model.m_modelMatrix.lights = m_modelMatrix.lights;
                    model.m_modelMatrix.material = m_modelMatrix.material;
                }
            }
        }

        public override void UpdateDraw()
        {
            if (!initialized) return;

            VulkanAPI.vkDeviceWaitIdle(device);
            
            List<CModel>models = UpdateDrawOrder();//按透明排序

            ResetModelUniformMatrixs(models);
            UpdateCommand(models);

            drawFrame();
        }

        private bool drawFrame()
        {
            if (commandBuffers == null) return false;
            if (commandBuffers.Length < 1) return false;

            uint imageIndex;
            ulong timeout = 1000000;
            SharpVulkan.VkResult result = VulkanAPI.vkAcquireNextImageKHR(device,
                            swapChain, timeout, imageAvailableSemaphore, null, out imageIndex);
            if ((int)result == (int)glfw3.VkResult.VK_ERROR_OUT_OF_DATE_KHR)
            {
                recreateSwapChain();
                return false;
            }
            else if (result != SharpVulkan.VkResult.VK_SUCCESS && (int)result != (int)glfw3.VkResult.VK_SUBOPTIMAL_KHR)
            {
                errMessage = "failed to acquire swap chain image!";
                return false;
            }

            VkSubmitInfo submitInfo = new VkSubmitInfo();
            submitInfo.waitSemaphores = new[] { imageAvailableSemaphore };
            submitInfo.waitDstStageMask = new VkPipelineStageFlags[]
            {
                //(VkPipelineStageFlags)0x00000400
                VkPipelineStageFlags.VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT 
            };
            
            submitInfo.commandBuffers = new VkCommandBuffer[] { commandBuffers[imageIndex] };
            VkSemaphore[] signalSemaphores = new[] { renderFinishedSemaphore };
            submitInfo.signalSemaphores = signalSemaphores;
            if (VulkanAPI.vkQueueSubmit(graphicsQueue, submitInfo, null) != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "failed to submit draw command buffer!";
                return false;
            }
            submitInfo.waitDstStageMask = null;
            submitInfo.waitSemaphores = null;
            submitInfo.commandBuffers = null;
            submitInfo = null;

            //presentation
            VkPresentInfoKHR2 presentInfo = new VkPresentInfoKHR2();
            presentInfo.waitSemaphores = signalSemaphores;
            VkSwapchainKHR[] swapChains = new[] { swapChain };
            presentInfo.swapchains = swapChains;
            presentInfo.imageIndices = new uint[] { imageIndex };
            result = VulkanAPI.vkQueuePresentKHR(presentQueue, ref presentInfo);
            if ((int)result == (int)glfw3.VkResult.VK_ERROR_OUT_OF_DATE_KHR ||
                 (int)result == (int)glfw3.VkResult.VK_SUBOPTIMAL_KHR)
            {
                recreateSwapChain();
            }
            else if (result != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "failed to present swap chain image!";
                return false;
            }
            VulkanAPI.vkQueueWaitIdle(presentQueue);

            errMessage = "present swap chain image successfully!";

            return true;
        }
        private void CleanGraphicsPipelines()
        {
            for (int i = 0; i < graphicsPipelines.Count; i++)
            {
                if (graphicsPipelines[i] != null)
                {
                    VulkanAPI.vkDestroyPipeline(device, graphicsPipelines[i]);
                    graphicsPipelines[i] = null;
                }
            }
            if (pipelineLayout != null)
            {
                VulkanAPI.vkDestroyPipelineLayout(device, pipelineLayout);
                pipelineLayout = null;
            }

            graphicsPipelines.Clear();
            graphicsPipeline = null;

            pipelineInfos.Clear();
            //pipelineInfo = null;
        }

        private void cleanupSwapChain()
        {
            if (depthImageView != null)
            {
                VulkanAPI.vkDestroyImageView(device, depthImageView);
                depthImageView = null;
            }
            if (depthImage != null)
            {
                VulkanAPI.vkDestroyImage(device, depthImage);
                depthImage = null;
            }
            if (depthImageMemory != null)
            {
                VulkanAPI.vkFreeMemory(device, depthImageMemory);
                depthImageMemory = null;
            }
            if (swapChainFramebuffers != null)
            {
                for (int i = 0; i < swapChainFramebuffers.Length; i++)
                {
                    if (swapChainFramebuffers[i] != null)
                    {
                        VulkanAPI.vkDestroyFramebuffer(device, swapChainFramebuffers[i]);
                        swapChainFramebuffers[i] = null;
                    }
                }
                swapChainFramebuffers = null;
            }
            if (commandBuffers != null)
            {
                VulkanAPI.vkFreeCommandBuffers(device, commandPool, commandBuffers);
                commandBuffers = null;
            }

            //CleanGraphicsPipelines();


            if (renderPass != null)
            {
                VulkanAPI.vkDestroyRenderPass(device, renderPass);
                renderPass = null;
            }
            if (swapChainImageViews != null)
            {
                for (int i = 0; i < swapChainImageViews.Length; i++)
                {
                    VulkanAPI.vkDestroyImageView(device, swapChainImageViews[i]);
                }
                swapChainImageViews = null;
            }
            if (swapChain != null)
            {
                VulkanAPI.vkDestroySwapchainKHR(device, swapChain);
                swapChain = null;
            }
        }
        public bool LoadShader(string vertexSpvFile, string fragSpvFile)
        {
            var vertShaderCode = ReadSpvFile(vertexSpvFile);
            var fragShaderCode = ReadSpvFile(fragSpvFile);
            if (vertShaderCode == null || fragShaderCode == null)
            {
                errMessage = "read spv file failed";
                return false;
            }
            //create shader module
            vertShaderModule = createShaderModule(vertShaderCode);
            fragShaderModule = createShaderModule(fragShaderCode);
            if (vertShaderModule == null || fragShaderModule == null)
            {
                errMessage = "failed to create shader module!";
                return false;
            }
            return true;
        }
        private bool createDescriptorSetLayout()
        {
            VkDescriptorSetLayoutBinding uboLayoutBinding = new VkDescriptorSetLayoutBinding();
            uboLayoutBinding.binding = 0;
            uboLayoutBinding.descriptorCount = 1;
            uboLayoutBinding.descriptorType = VkDescriptorType.VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
            uboLayoutBinding.immutableSamplers = null;
            uboLayoutBinding.stageFlags = VkShaderStageFlagBits.VK_SHADER_STAGE_VERTEX_BIT;
            VkDescriptorSetLayoutBinding samplerLayoutBinding = new VkDescriptorSetLayoutBinding();
            samplerLayoutBinding.binding = 1;
            samplerLayoutBinding.descriptorCount = 1;
            samplerLayoutBinding.descriptorType = VkDescriptorType.VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER;
            samplerLayoutBinding.immutableSamplers = null;
            samplerLayoutBinding.stageFlags = VkShaderStageFlagBits.VK_SHADER_STAGE_FRAGMENT_BIT;

            VkDescriptorSetLayoutCreateInfo layoutInfo = new VkDescriptorSetLayoutCreateInfo();
            layoutInfo.bindings = new[] { uboLayoutBinding, samplerLayoutBinding };

            if (VulkanAPI.vkCreateDescriptorSetLayout(device, ref layoutInfo, out descriptorSetLayout)
                != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "failed to create descriptor set layout!";
                return false;
            }
            return true;
        }
        private bool createDescriptorPool()
        {
            //VkDescriptorPoolSize poolSize = new VkDescriptorPoolSize();
            VkDescriptorPoolSize[] poolSizes = new VkDescriptorPoolSize[2];
            poolSizes[0].type = VkDescriptorType.VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
            poolSizes[0].descriptorCount = 1;
            poolSizes[1].type = VkDescriptorType.VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER;
            poolSizes[1].descriptorCount = 1;
            VkDescriptorPoolCreateInfo poolInfo = new VkDescriptorPoolCreateInfo();
            //poolInfo.poolSizes = new[] { poolSize };
            poolInfo.poolSizes = poolSizes;
            poolInfo.maxSets = 1;
            if (VulkanAPI.vkCreateDescriptorPool(device, ref poolInfo, out descriptorPool)
                != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "failed to create descriptor pool!";
                return false;
            }
            return true;
        }
        private bool createDescriptorSet()
        {
            VkDescriptorSetAllocateInfo allocInfo = new VkDescriptorSetAllocateInfo();
            allocInfo.descriptorPool = descriptorPool;
            allocInfo.allocateSetLayouts = new[] { descriptorSetLayout };
            VkDescriptorSet[] descriptorSets;
            if (VulkanAPI.vkAllocateDescriptorSets(device, ref allocInfo, out descriptorSets)
                != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "failed to allocate descriptor set!";
                return false;
            }
            descriptorSet = descriptorSets[0];
            UpdateDescriptorSet();
            return true;
        }
        private void UpdateDescriptorSet()
        {
            if (descriptorSet == null) return;
            if (uniformBuffer == null) return;
            VkDescriptorBufferInfo bufferInfo = new VkDescriptorBufferInfo();
            bufferInfo.buffer = uniformBuffer;
            bufferInfo.offset = 0;
            bufferInfo.range = UniformBufferObject.GetSize();

            VkDescriptorImageInfo imageInfo = new VkDescriptorImageInfo();
            imageInfo.imageLayout = VkImageLayout.VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
            imageInfo.imageView = textureImageView;
            imageInfo.sampler = textureSampler;

            //VkWriteDescriptorSet descriptorWrite = new VkWriteDescriptorSet();
            VkWriteDescriptorSet[] descriptorWrites = new VkWriteDescriptorSet[2];
            descriptorWrites[0] = new VkWriteDescriptorSet();
            descriptorWrites[0].dstSet = descriptorSet;
            descriptorWrites[0].dstBinding = 0;
            descriptorWrites[0].dstArrayElement = 0;
            descriptorWrites[0].descriptorType = VkDescriptorType.VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
            descriptorWrites[0].descriptorCount = 1;
            descriptorWrites[0].pBufferInfo = new[] { bufferInfo };
            descriptorWrites[0].pImageInfo = null; // Optional
            descriptorWrites[0].pTexelBufferView = null; // Optional
            descriptorWrites[1] = new VkWriteDescriptorSet();
            descriptorWrites[1].dstSet = descriptorSet;
            descriptorWrites[1].dstBinding = 1;
            descriptorWrites[1].dstArrayElement = 0;
            descriptorWrites[1].descriptorType = VkDescriptorType.VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER;
            descriptorWrites[1].descriptorCount = 1;
            descriptorWrites[1].pImageInfo = new[] { imageInfo };

            //VulkanAPI.vkUpdateDescriptorSets(device, new[] { descriptorWrite }, null);
            VulkanAPI.vkUpdateDescriptorSets(device, descriptorWrites, null);
        }
        private bool CreatePipelineLayout()
        {
            if (pipelineLayout == null)
            {
                if (descriptorSetLayout == null) createDescriptorSetLayout();
                if (descriptorSetLayout == null) return false;
                VkPipelineLayoutCreateInfo pipelineLayoutInfo = new VkPipelineLayoutCreateInfo();
                //pipelineLayoutInfo.setLayouts = null;
                pipelineLayoutInfo.setLayouts = new[] { descriptorSetLayout };  //JIAN debug
                if (VulkanAPI.vkCreatePipelineLayout(device, ref pipelineLayoutInfo, out pipelineLayout)
                    != SharpVulkan.VkResult.VK_SUCCESS)
                {
                    errMessage = "failed to create pipeline layout!";
                    return false;
                }
            }
            return true;
        }

        public void SetViewportState(VkViewport _vewport, VkRect2D _scissor)
        {
            viewportState.viewports = new VkViewport[] { _vewport };
            viewportState.scissors = new VkRect2D[] { _scissor };
        }
        public void SetViewportState(VkPipelineViewportStateCreateInfo _viewportState)
        {
            viewportState = _viewportState;
        }
        private void ResetViewport()
        {
            viewportState.viewports[0].width = windowWidth;
            viewportState.viewports[0].height = windowHeight;
            viewportState.scissors[0].extent.width = (uint)windowWidth;
            viewportState.scissors[0].extent.height = (uint)windowHeight;
            pipelineInfo.viewportState = viewportState;
        }
        private void UpdateCurrentViewport(VkCommandBuffer cmdBuffer)
        {
            VulkanAPI.vkCmdSetViewport(cmdBuffer, 0, viewportState.viewports);
            VulkanAPI.vkCmdSetScissor(cmdBuffer, 0, viewportState.scissors);
        }
        public void SetPrimitive(VkPrimitiveTopology _primitive = VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST)
        {
            //VK_PRIMITIVE_TOPOLOGY_POINT_LIST = 0,
            //VK_PRIMITIVE_TOPOLOGY_LINE_LIST = 1,
            //VK_PRIMITIVE_TOPOLOGY_LINE_STRIP = 2,
            //VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST = 3,
            //VK_PRIMITIVE_TOPOLOGY_TRIANGLE_STRIP = 4,
            //VK_PRIMITIVE_TOPOLOGY_TRIANGLE_FAN = 5,
            //VK_PRIMITIVE_TOPOLOGY_LINE_LIST_WITH_ADJACENCY = 6,
            //VK_PRIMITIVE_TOPOLOGY_LINE_STRIP_WITH_ADJACENCY = 7,
            //VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST_WITH_ADJACENCY = 8,
            //VK_PRIMITIVE_TOPOLOGY_TRIANGLE_STRIP_WITH_ADJACENCY = 9,
            //VK_PRIMITIVE_TOPOLOGY_PATCH_LIST = 10            
            //
            inputAssembly.topology = _primitive;

            //If topology is follow values,primitiveRestartEnable must be VK_FALSE
            if ((_primitive == VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_POINT_LIST) ||
                 (_primitive == VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_LINE_LIST) ||
                 (_primitive == VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST) ||
                 (_primitive == VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_LINE_LIST_WITH_ADJACENCY) ||
                 (_primitive == VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST_WITH_ADJACENCY) ||
                 (_primitive == VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_PATCH_LIST))
                inputAssembly.primitiveRestartEnable = false;

            pipelineInfo.inputAssemblyState = inputAssembly;

            if (_primitive == VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_POINT_LIST)
            {
                SetPolygonMode(VkPolygonMode.VK_POLYGON_MODE_POINT);
            }
            else if (_primitive == VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_LINE_LIST ||
                _primitive == VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_LINE_STRIP ||
                _primitive == VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_LINE_LIST_WITH_ADJACENCY ||
                _primitive == VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_LINE_STRIP_WITH_ADJACENCY)
            {
                SetPolygonMode(VkPolygonMode.VK_POLYGON_MODE_LINE);
            }

        }
        public void SetPrimitive(DrawingPrimitive _primitive)
        {
            SetPrimitive(EnumConvert(_primitive));
        }
        private VkCullModeFlags EnumConvert(CullModeEnum mode)
        {
            switch (mode)
            {
                case CullModeEnum.NONE:
                    return VkCullModeFlags.VK_CULL_MODE_NONE;
                case CullModeEnum.BACK:
                    return VkCullModeFlags.VK_CULL_MODE_BACK_BIT;
                case CullModeEnum.FRONT:
                    return VkCullModeFlags.VK_CULL_MODE_FRONT_BIT;
                case CullModeEnum.FRONT_AND_BACK:
                    return VkCullModeFlags.VK_CULL_MODE_FRONT_AND_BACK;
            }
            return VkCullModeFlags.VK_CULL_MODE_NONE;
        }
        private VkFrontFace EnumConvert(FrontFaceOder mode)
        {
            switch (mode)
            {
                case FrontFaceOder.CLOCKWISE:
                    return VkFrontFace.VK_FRONT_FACE_CLOCKWISE;
                case FrontFaceOder.COUNTER_CLOCKWISE:
                    return VkFrontFace.VK_FRONT_FACE_COUNTER_CLOCKWISE;
            }
            return VkFrontFace.VK_FRONT_FACE_COUNTER_CLOCKWISE;
        }
        private VkPrimitiveTopology EnumConvert(DrawingPrimitive _primitive)
        {
            switch (_primitive)
            {
                case DrawingPrimitive.POINT_LIST:
                    return VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_POINT_LIST;
                case DrawingPrimitive.LINE_LIST:
                    return VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_LINE_LIST;
                case DrawingPrimitive.LINE_STRIP:
                    return VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_LINE_STRIP;
                case DrawingPrimitive.TRIANGLE_LIST:
                    return VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST;
                case DrawingPrimitive.TRIANGLE_STRIP:
                    return VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_TRIANGLE_STRIP;
                case DrawingPrimitive.TRIANGLE_FAN:
                    return VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_TRIANGLE_FAN;
                case DrawingPrimitive.LINE_LIST_WITH_ADJACENCY:
                    return VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_LINE_LIST_WITH_ADJACENCY;
                case DrawingPrimitive.LINE_STRIP_WITH_ADJACENCY:
                    return VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_LINE_STRIP_WITH_ADJACENCY;
                case DrawingPrimitive.TRIANGLE_LIST_WITH_ADJACENCY:
                    return VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST_WITH_ADJACENCY;
                case DrawingPrimitive.TRIANGLE_STRIP_WITH_ADJACENCY:
                    return VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_TRIANGLE_STRIP_WITH_ADJACENCY;
                case DrawingPrimitive.PATCH_LIST:
                    return VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_PATCH_LIST;
                default: return VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST;
            }
        }

        private VkPipelineDynamicStateCreateInfo CreatePipelineDynamicStateCreateInfo()
        {
            //set dynamic state
            VkDynamicState[] states = new VkDynamicState[]
            {
                VkDynamicState.VK_DYNAMIC_STATE_VIEWPORT,
                VkDynamicState.VK_DYNAMIC_STATE_SCISSOR,
                VkDynamicState.VK_DYNAMIC_STATE_LINE_WIDTH,
              //  VkDynamicState.VK_DYNAMIC_STATE_DEPTH_BIAS,
              //  VkDynamicState.VK_DYNAMIC_STATE_BLEND_CONSTANTS,
              //  VkDynamicState.VK_DYNAMIC_STATE_DEPTH_BOUNDS,
              //  VkDynamicState.VK_DYNAMIC_STATE_STENCIL_COMPARE_MASK,
              //  VkDynamicState.VK_DYNAMIC_STATE_STENCIL_WRITE_MASK,
              //  VkDynamicState.VK_DYNAMIC_STATE_STENCIL_REFERENCE
            };
            VkPipelineDynamicStateCreateInfo p = new VkPipelineDynamicStateCreateInfo();
            p.dynamicStateCount = (uint)states.Length;
            p.pDynamicStates = ArrayToIntPtr(states);
            //end set dynamic state
            return p;
        }
        //update current Pipeline Parameters
        private void UpdateDynamicPipelinePara(VkCommandBuffer cmd, CModelDescriptor model)
        {
            VulkanAPI.vkCmdSetLineWidth(cmd, model.pipelineInfo.pRasterizationState.lineWidth);
            VulkanAPI.vkCmdSetViewport(cmd, 0, pipelineInfo.viewportState.viewports);
            VulkanAPI.vkCmdSetScissor(cmd, 0, pipelineInfo.viewportState.scissors);

            //VulkanAPI.vkCmdSetDepthBias(cmd,);
            //  VulkanAPI.vkCmdSetBlendConstants(cmd,);
            //   VulkanAPI.vkCmdSetDepthBounds(cmd);
            //  VulkanAPI.vkCmdSetStencilCompareMask(cmd);
            //   VulkanAPI.vkCmdSetStencilWriteMask(cmd);
            //   VulkanAPI.vkCmdSetStencilReference(cmd, VkStencilFaceFlags);
        }

        private void CreateDefaultInputAssembly()
        {
            inputAssembly = new VkPipelineInputAssemblyStateCreateInfo();
            inputAssembly.topology = VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST;
            inputAssembly.primitiveRestartEnable = false;
        }
        private VkPipelineInputAssemblyStateCreateInfo SaveInputAssembly(VkPipelineInputAssemblyStateCreateInfo p)
        {
            VkPipelineInputAssemblyStateCreateInfo p1 = new VkPipelineInputAssemblyStateCreateInfo();
            p1.topology = p.topology;
            p1.primitiveRestartEnable = p.primitiveRestartEnable;
            return p1;
        }
        private bool IsSameParameter(VkPipelineInputAssemblyStateCreateInfo p1, VkPipelineInputAssemblyStateCreateInfo p2)
        {
            if (p1.topology != p2.topology) return false;
            if (p1.primitiveRestartEnable != p2.primitiveRestartEnable) return false;
            return true;
        }
        private void CreateDefaultRasterizer()
        {
            rasterizer = new VkPipelineRasterizationStateCreateInfo();
            rasterizer.depthClampEnable = false;
            rasterizer.rasterizerDiscardEnable = false;
            rasterizer.polygonMode = VkPolygonMode.VK_POLYGON_MODE_FILL;
            //rasterizer.polygonMode = VkPolygonMode.VK_POLYGON_MODE_LINE;
            rasterizer.lineWidth = 1.0f;
            rasterizer.cullMode = VkCullModeFlags.VK_CULL_MODE_NONE;
            //rasterizer.cullMode = VkCullModeFlags.VK_CULL_MODE_BACK_BIT;
            //rasterizer.cullMode = VkCullModeFlags.VK_CULL_MODE_FRONT_BIT;
            //rasterizer.cullMode = VkCullModeFlags.VK_CULL_MODE_FRONT_AND_BACK;
            //rasterizer.cullMode = VkCullModeFlags.VK_CULL_MODE_NONE;
            //rasterizer.frontFace = VkFrontFace.VK_FRONT_FACE_CLOCKWISE;
            rasterizer.frontFace = VkFrontFace.VK_FRONT_FACE_COUNTER_CLOCKWISE;
            rasterizer.depthBiasEnable = false;

        }
        private VkPipelineRasterizationStateCreateInfo SaveRasterizer(VkPipelineRasterizationStateCreateInfo p)
        {
            VkPipelineRasterizationStateCreateInfo p1 = new VkPipelineRasterizationStateCreateInfo();
            p1.depthClampEnable = p.depthClampEnable;
            p1.rasterizerDiscardEnable = p.rasterizerDiscardEnable;
            p1.polygonMode = p.polygonMode;
            p1.lineWidth = p.lineWidth;
            p1.cullMode = p.cullMode;
            p1.frontFace = p.frontFace;
            p1.depthBiasEnable = p.depthBiasEnable;
            return p1;
        }
        private bool IsSameParameter(VkPipelineRasterizationStateCreateInfo p1, VkPipelineRasterizationStateCreateInfo p2)
        {
            if (p1.depthClampEnable != p2.depthClampEnable) return false;
            if (p1.rasterizerDiscardEnable != p2.rasterizerDiscardEnable) return false;
            if (p1.polygonMode != p2.polygonMode) return false;

            //lineWidth specified by cmd dynamic  
            //if (p1.lineWidth != p2.lineWidth) return false;

            if (p1.cullMode != p2.cullMode) return false;
            if (p1.frontFace != p2.frontFace) return false;
            if (p1.depthBiasEnable != p2.depthBiasEnable) return false;
            return true;
        }
        public void SetRasterizationState(VkPipelineRasterizationStateCreateInfo _rasterizer)
        {
            rasterizer = _rasterizer;
            pipelineInfo.pRasterizationState = rasterizer;
        }
        public void SetRasterizationState(float _linewidth = 1.0f,
                                          VkPolygonMode _polygonMode = VkPolygonMode.VK_POLYGON_MODE_FILL,
                                          VkCullModeFlags _culllMode = VkCullModeFlags.VK_CULL_MODE_BACK_BIT,
                                          VkFrontFace _frontFace = VkFrontFace.VK_FRONT_FACE_COUNTER_CLOCKWISE)
        {
            rasterizer.depthClampEnable = false;
            rasterizer.rasterizerDiscardEnable = false;
            rasterizer.polygonMode = _polygonMode;
            rasterizer.lineWidth = _linewidth;
            m_LineWidth = _linewidth;
            rasterizer.cullMode = _culllMode;
            rasterizer.frontFace = _frontFace;
            rasterizer.depthBiasEnable = false;

            pipelineInfo.pRasterizationState.depthClampEnable = false;
            pipelineInfo.pRasterizationState.rasterizerDiscardEnable = false;
            pipelineInfo.pRasterizationState.polygonMode = _polygonMode;
            pipelineInfo.pRasterizationState.lineWidth = _linewidth;
            pipelineInfo.pRasterizationState.cullMode = _culllMode;
            pipelineInfo.pRasterizationState.frontFace = _frontFace;
            pipelineInfo.pRasterizationState.depthBiasEnable = false;
        }
        public void SetCullMode(CullModeEnum mode = CullModeEnum.NONE, FrontFaceOder order = FrontFaceOder.COUNTER_CLOCKWISE)
        {
            rasterizer.cullMode = EnumConvert(mode);
            rasterizer.frontFace = EnumConvert(order);
            pipelineInfo.pRasterizationState.cullMode = rasterizer.cullMode;
            pipelineInfo.pRasterizationState.frontFace = rasterizer.frontFace;
        }
        public override void SetPointSize(double _size = 1.0)
        {
            base.SetPointSize(_size);
        }
        public override void SetLineWidth(double _linewidth = 1.0)
        {
            base.SetLineWidth(_linewidth);
            rasterizer.lineWidth = (float)_linewidth;
            pipelineInfo.pRasterizationState.lineWidth = (float)_linewidth;
        }
        public override void SetPolygonMode(gDrawMode _polygonMode)
        {
            base.SetPolygonMode(_polygonMode);

            if (_polygonMode == gDrawMode.Fill)
                SetPolygonMode(VkPolygonMode.VK_POLYGON_MODE_FILL);
            else if (_polygonMode == gDrawMode.Points)
                SetPolygonMode(VkPolygonMode.VK_POLYGON_MODE_POINT);
            else if (_polygonMode == gDrawMode.Wireframe)
                SetPolygonMode(VkPolygonMode.VK_POLYGON_MODE_LINE);
        }
        public void SetPolygonMode(VkPolygonMode _polygonMode = VkPolygonMode.VK_POLYGON_MODE_FILL)
        {
            rasterizer.polygonMode = _polygonMode;
            pipelineInfo.pRasterizationState.polygonMode = _polygonMode;
        }
        private void CreateDefaultViewportState()
        {
            //Viewports and scissors
            VkViewport viewport = new VkViewport();
            viewport.x = 0.0f;
            viewport.y = 0.0f;
            viewport.width = swapChainExtent.width;
            viewport.height = swapChainExtent.height;
            viewport.minDepth = 0.0f;
            viewport.maxDepth = 1.0f;
            VkRect2D scissor = new VkRect2D();
            scissor.offset.x = 0;
            scissor.offset.y = 0;
            scissor.extent = new VkExtent2D()
            {
                width = swapChainExtent.width,
                height = swapChainExtent.height
            };
            viewportState = new VkPipelineViewportStateCreateInfo();
            viewportState.viewports = new VkViewport[] { viewport };
            viewportState.scissors = new VkRect2D[] { scissor };
        }
        private VkPipelineViewportStateCreateInfo SaveViewportState(VkPipelineViewportStateCreateInfo p)
        {
            VkPipelineViewportStateCreateInfo p1 = new VkPipelineViewportStateCreateInfo();
            p1.viewports = new VkViewport[1];
            p1.scissors = new VkRect2D[1];

            p1.viewports[0].x = p.viewports[0].x;
            p1.viewports[0].y = p.viewports[0].y;
            p1.viewports[0].width = p.viewports[0].width;
            p1.viewports[0].height = p.viewports[0].height;
            p1.viewports[0].minDepth = p.viewports[0].minDepth;
            p1.viewports[0].maxDepth = p.viewports[0].maxDepth;
            p1.scissors[0].offset.x = p.scissors[0].offset.x;
            p1.scissors[0].offset.y = p.scissors[0].offset.y;
            p1.scissors[0].extent.width = p.scissors[0].extent.width;
            p1.scissors[0].extent.height = p.scissors[0].extent.height;
            return p1;
        }
        private bool IsSameParameter(VkPipelineViewportStateCreateInfo p1, VkPipelineViewportStateCreateInfo p2)
        {
            return true;

#pragma warning disable CS0162 // 检测到无法访问的代码
            if (p1.viewports[0].x != p2.viewports[0].x) return false;
#pragma warning restore CS0162 // 检测到无法访问的代码
            if (p1.viewports[0].y != p2.viewports[0].y) return false;
            if (p1.viewports[0].width != p2.viewports[0].width) return false;
            if (p1.viewports[0].height != p2.viewports[0].height) return false;
            if (p1.viewports[0].minDepth != p2.viewports[0].minDepth) return false;
            if (p1.viewports[0].maxDepth != p2.viewports[0].maxDepth) return false;
            if (p1.scissors[0].offset.x != p2.scissors[0].offset.x) return false;
            if (p1.scissors[0].offset.y != p2.scissors[0].offset.y) return false;
            if (p1.scissors[0].extent.width != p2.scissors[0].extent.width) return false;
            if (p1.scissors[0].extent.height != p2.scissors[0].extent.height) return false;
            return true;
        }
        private void CreateDefaultMultiSampling()
        {
            multiSampling = new VkPipelineMultisampleStateCreateInfo();
            multiSampling.sampleShadingEnable = false;
            multiSampling.rasterizationSamples = VkSampleCountFlags.VK_SAMPLE_COUNT_1_BIT;
            multiSampling.minSampleShading = 1.0f;  //optional
            multiSampling.alphaToCoverageEnable = false;  //optional
            multiSampling.alphaToOneEnable = false;   //optional
        }
        private VkPipelineMultisampleStateCreateInfo SaveMultiSampling(VkPipelineMultisampleStateCreateInfo p)
        {
            VkPipelineMultisampleStateCreateInfo p1 = new VkPipelineMultisampleStateCreateInfo();
            p1.sampleShadingEnable = p.sampleShadingEnable;
            p1.rasterizationSamples = p.rasterizationSamples;
            p1.minSampleShading = p.minSampleShading;
            p1.alphaToCoverageEnable = p.alphaToCoverageEnable;
            p1.alphaToOneEnable = p.alphaToOneEnable;
            return p1;
        }
        private bool IsSameParameter(VkPipelineMultisampleStateCreateInfo p1, VkPipelineMultisampleStateCreateInfo p2)
        {
            if (p1.sampleShadingEnable != p2.sampleShadingEnable) return false;
            if (p1.rasterizationSamples != p2.rasterizationSamples) return false;
            if (p1.minSampleShading != p2.minSampleShading) return false;
            if (p1.alphaToCoverageEnable != p2.alphaToCoverageEnable) return false;
            if (p1.alphaToOneEnable != p2.alphaToOneEnable) return false;
            return true;
        }
        private void CreateDefaultColorBlending()
        {
            VkPipelineColorBlendAttachmentState colorBlendAttachment = new VkPipelineColorBlendAttachmentState();
            colorBlendAttachment.colorWriteMask = VkColorComponentFlags.VK_COLOR_COMPONENT_R_BIT |
                                                      VkColorComponentFlags.VK_COLOR_COMPONENT_G_BIT |
                                                      VkColorComponentFlags.VK_COLOR_COMPONENT_B_BIT |
                                                      VkColorComponentFlags.VK_COLOR_COMPONENT_A_BIT;
            //第一种混合模式true
            colorBlendAttachment.blendEnable = true;
            colorBlendAttachment.alphaBlendOp = VkBlendOp.VK_BLEND_OP_ADD;
            colorBlendAttachment.colorBlendOp = VkBlendOp.VK_BLEND_OP_ADD;
            colorBlendAttachment.srcColorBlendFactor = VkBlendFactor.VK_BLEND_FACTOR_SRC_ALPHA;
            colorBlendAttachment.dstColorBlendFactor = VkBlendFactor.VK_BLEND_FACTOR_ONE_MINUS_SRC_ALPHA;
            colorBlendAttachment.srcAlphaBlendFactor = VkBlendFactor.VK_BLEND_FACTOR_ONE;
            colorBlendAttachment.dstAlphaBlendFactor = VkBlendFactor.VK_BLEND_FACTOR_ZERO;
            //第二种混合模式false
            colorBlending = new VkPipelineColorBlendStateCreateInfo();
            colorBlending.logicOpEnable = false;
            colorBlending.logicOp = VkLogicOp.VK_LOGIC_OP_COPY;
            colorBlending.attachments = new VkPipelineColorBlendAttachmentState[] { colorBlendAttachment };
            colorBlending.blendConstants = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };

        }
        private VkPipelineColorBlendStateCreateInfo SaveColorBlending(VkPipelineColorBlendStateCreateInfo p)
        {
            VkPipelineColorBlendStateCreateInfo p1 = new VkPipelineColorBlendStateCreateInfo();
            p1.blendConstants = new float[4];
            p1.attachments = new VkPipelineColorBlendAttachmentState[1];
            p1.attachments[0] = new VkPipelineColorBlendAttachmentState();
            p1.attachments[0].colorWriteMask = p.attachments[0].colorWriteMask;
            p1.attachments[0].blendEnable = p.attachments[0].blendEnable;
            p1.attachments[0].alphaBlendOp = p.attachments[0].alphaBlendOp;
            p1.attachments[0].colorBlendOp = p.attachments[0].colorBlendOp;
            p1.attachments[0].srcColorBlendFactor = p.attachments[0].srcColorBlendFactor;
            p1.attachments[0].dstColorBlendFactor = p.attachments[0].dstColorBlendFactor;
            p1.attachments[0].srcAlphaBlendFactor = p.attachments[0].srcAlphaBlendFactor;
            p1.attachments[0].dstAlphaBlendFactor = p.attachments[0].dstAlphaBlendFactor;

            for (int i = 0; i < 4; i++)
                p1.blendConstants[i] = p.blendConstants[i];

            p1.logicOpEnable = p.logicOpEnable;
            p1.logicOp = p.logicOp;
            return p1;
        }
        private bool IsSameParameter(VkPipelineColorBlendStateCreateInfo p1, VkPipelineColorBlendStateCreateInfo p2)
        {
            if (p1.attachments[0].colorWriteMask != p2.attachments[0].colorWriteMask) return false;
            if (p1.attachments[0].blendEnable != p2.attachments[0].blendEnable) return false;
            if (p1.attachments[0].alphaBlendOp != p2.attachments[0].alphaBlendOp) return false;
            if (p1.attachments[0].colorBlendOp != p2.attachments[0].colorBlendOp) return false;
            if (p1.attachments[0].srcColorBlendFactor != p2.attachments[0].srcColorBlendFactor) return false;
            if (p1.attachments[0].dstColorBlendFactor != p2.attachments[0].dstColorBlendFactor) return false;
            if (p1.attachments[0].srcAlphaBlendFactor != p2.attachments[0].srcAlphaBlendFactor) return false;
            if (p1.attachments[0].dstAlphaBlendFactor != p2.attachments[0].dstAlphaBlendFactor) return false;

            for (int i = 0; i < 4; i++)
                if (p1.blendConstants[i] != p2.blendConstants[i]) return false;
            if (p1.logicOpEnable != p2.logicOpEnable) return false;
            if (p1.logicOp != p2.logicOp) return false;
            return true;
        }

        public IntPtr ArrayToIntPtr(VkDynamicState[] array)
        {
            if (array == null || array.Length == 0)
                return IntPtr.Zero;

            int[] ret = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
                ret[i] = (int)array[i];

            int size = sizeof(int);
            IntPtr lp = System.Runtime.InteropServices.Marshal.AllocHGlobal(size * ret.Length);
            IntPtr tmp = lp;
            for (int i = 0; i < ret.Length; i++, tmp += size)
                System.Runtime.InteropServices.Marshal.StructureToPtr(ret[i], tmp, false);

            return lp;
        }

        private bool CreateDefaultPipelineInfo()
        {
            if (vertShaderModule == null || fragShaderModule == null)
            {
                errMessage = "error: shader module is null";
                return false;
            }
            VkPipelineShaderStageCreateInfo vertShaderStageInfo = new VkPipelineShaderStageCreateInfo();
            vertShaderStageInfo.stage = VkShaderStageFlagBits.VK_SHADER_STAGE_VERTEX_BIT;
            vertShaderStageInfo.module = vertShaderModule;
            vertShaderStageInfo.pName = "main";

            VkPipelineShaderStageCreateInfo fragShaderStageInfo = new VkPipelineShaderStageCreateInfo();
            fragShaderStageInfo.stage = VkShaderStageFlagBits.VK_SHADER_STAGE_FRAGMENT_BIT;
            fragShaderStageInfo.module = fragShaderModule;
            fragShaderStageInfo.pName = "main";
            //Shader stage creation
            VkPipelineShaderStageCreateInfo[] shaderStages = new[] { vertShaderStageInfo, fragShaderStageInfo };

            //Vertex input description
            VkPipelineVertexInputStateCreateInfo vertexInputInfo = new VkPipelineVertexInputStateCreateInfo();
            vertexInputInfo.flags = new VkPipelineVertexInputStateCreateFlags();
            vertexInputInfo.attributeDescriptions = getAttributeDescriptions();
            vertexInputInfo.bindingDescriptions = getBindingDescription();

            VkPipelineDepthStencilStateCreateInfo depthStencil = new VkPipelineDepthStencilStateCreateInfo();
            depthStencil.depthTestEnable = true;
            depthStencil.depthWriteEnable = true;
            depthStencil.depthCompareOp = VkCompareOp.VK_COMPARE_OP_LESS;
            depthStencil.depthBoundsTestEnable = false;
            depthStencil.minDepthBounds = 0.0f; // Optional
            depthStencil.maxDepthBounds = 1.0f; // Optional
            depthStencil.stencilTestEnable = false;
            //depthStencil.front = { }; // Optional
            //depthStencil.back = { }; // Optional

            //create shader module,use LoadShader()..
            //Input assembly..
            if (inputAssembly == null) CreateDefaultInputAssembly();
            //viewport state
            if (viewportState == null) CreateDefaultViewportState();
            //Rasterizer
            if (rasterizer == null) CreateDefaultRasterizer();
            //Multisample
            if (multiSampling == null) CreateDefaultMultiSampling();
            //Color blending
            if (colorBlending == null) CreateDefaultColorBlending();

            pipelineInfo = new VkGraphicsPipelineCreateInfo();
            //set dynamic state
            pipelineInfo.pDynamicState = CreatePipelineDynamicStateCreateInfo();

            pipelineInfo.pStages = shaderStages;
            pipelineInfo.vertexInputState = vertexInputInfo;
            pipelineInfo.inputAssemblyState = inputAssembly;
            pipelineInfo.viewportState = viewportState;
            pipelineInfo.pRasterizationState = rasterizer;
            pipelineInfo.pMultisampleState = multiSampling;
            pipelineInfo.pColorBlendState = colorBlending;
            pipelineInfo.tessellationState = null;//default not use tessellationState
            pipelineInfo.pDepthStencilState = depthStencil;
            pipelineInfo.layout = pipelineLayout;
            pipelineInfo.renderPass = renderPass;
            pipelineInfo.subpass = 0;

            return true;
        }

        private VkGraphicsPipelineCreateInfo CopyPipelineInfo(VkGraphicsPipelineCreateInfo p)
        {
            VkGraphicsPipelineCreateInfo p1 = new VkGraphicsPipelineCreateInfo();
            p1.pDynamicState = CreatePipelineDynamicStateCreateInfo();
            p1.pDepthStencilState = new VkPipelineDepthStencilStateCreateInfo();
            p1.tessellationState = new VkPipelineTessellationStateCreateInfo();
            p1.vertexInputState = new VkPipelineVertexInputStateCreateInfo();

            p1.basePipelineHandle = p.basePipelineHandle;
            p1.flags = p.flags;
            p1.subpass = p.subpass;
            p1.renderPass = p.renderPass;
            p1.layout = p.layout;
            p1.pDynamicState = p.pDynamicState;
            p1.pColorBlendState = SaveColorBlending(p.pColorBlendState);
            p1.basePipelineIndex = p.basePipelineIndex;
            p1.pDepthStencilState = CopyDepthStencilState(p.pDepthStencilState);
            p1.pRasterizationState = SaveRasterizer(p.pRasterizationState);
            p1.viewportState = SaveViewportState(p.viewportState);
            p1.tessellationState = p.tessellationState;
            p1.inputAssemblyState = SaveInputAssembly(p.inputAssemblyState);
            p1.vertexInputState = p.vertexInputState;
            p1.pStages = p.pStages;
            p1.pMultisampleState = SaveMultiSampling(p.pMultisampleState);

            return p1;
        }
        VkPipelineDepthStencilStateCreateInfo CopyDepthStencilState(VkPipelineDepthStencilStateCreateInfo p)
        {
            VkPipelineDepthStencilStateCreateInfo p1 = new VkPipelineDepthStencilStateCreateInfo();
            p1.depthWriteEnable = p.depthWriteEnable;
            p1.depthTestEnable = p.depthTestEnable;
            p1.depthCompareOp = p.depthCompareOp;
            p1.depthBoundsTestEnable = p.depthBoundsTestEnable;
            p1.back = p.back;
            p1.front = p.front;
            p1.flags = p.flags;
            p1.maxDepthBounds = p.maxDepthBounds;
            p1.minDepthBounds = p.minDepthBounds;
            return p1;
        }
        public override void SetDepthWriteEnable(bool enable = true)
        {
            pipelineInfo.pDepthStencilState.depthWriteEnable = enable;
        }
        public override void SetDepthTestEnable(bool enable = true)
        {
            pipelineInfo.pDepthStencilState.depthTestEnable = enable;
        }
        private bool IsSameParameter(VkGraphicsPipelineCreateInfo p1, VkGraphicsPipelineCreateInfo p2)
        {
            if (p1 == null && p2 != null) return false;
            if (p2 == null && p1 != null) return false;

            if (p1.pStages != p2.pStages) return false;
            if (p1.layout != p2.layout) return false;
            if (!IsSameParameter(p1.inputAssemblyState, p2.inputAssemblyState)) return false;
            if (!IsSameParameter(p1.viewportState, p2.viewportState)) return false;
            if (!IsSameParameter(p1.pRasterizationState, p2.pRasterizationState)) return false;
            if (!IsSameParameter(p1.pMultisampleState, p2.pMultisampleState)) return false;
            if (!IsSameParameter(p1.pColorBlendState, p2.pColorBlendState)) return false;

            return true;
        }
        // bug fixed, 从graphicsPipelines列表中检索对应的列表 2021-1-10
        //常用的几种
        private bool GetExistGraphicPipeline(VkGraphicsPipelineCreateInfo info)
        {
            graphicsPipeline = null;
            switch (info.inputAssemblyState.topology)
            {
                case VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_POINT_LIST:
                case VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_LINE_LIST:
                case VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_LINE_STRIP:
                case VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_LINE_LIST_WITH_ADJACENCY:
                case VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_LINE_STRIP_WITH_ADJACENCY:
                    for (int i = 0; i < pipelineInfos.Count; i++)
                    {
                        if ( info.inputAssemblyState.topology == pipelineInfos[i].inputAssemblyState.topology
                            //&& info.pRasterizationState.lineWidth == pipelineInfos[i].pRasterizationState.lineWidth
                            )
                        {
                            graphicsPipeline = graphicsPipelines[i];
                            return true;
                        }
                    }
                    break;
                case VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST:
                case VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_TRIANGLE_STRIP:
                case VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_TRIANGLE_FAN:
                case VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST_WITH_ADJACENCY:
                case VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_TRIANGLE_STRIP_WITH_ADJACENCY:
                    for (int i = 0; i < pipelineInfos.Count; i++)
                    {
                        if (info.inputAssemblyState.topology == pipelineInfos[i].inputAssemblyState.topology &&
                            info.pRasterizationState.polygonMode == pipelineInfos[i].pRasterizationState.polygonMode &&
                            info.pDepthStencilState.depthWriteEnable == pipelineInfos[i].pDepthStencilState.depthWriteEnable
                            //&&info.pRasterizationState.lineWidth == pipelineInfos[i].pRasterizationState.lineWidth 
                            )
                        {
                            graphicsPipeline = graphicsPipelines[i];
                            return true;
                        }
                    }
                    break;
            }
            return false;
        }
        /// <summary>
        /// create graphics pipelines-- all type objects 
        /// </summary>
        /// <returns></returns>
        private bool CreateDefaultGraphicsPipeLine()
        {
            CreateDefaultPipelineInfo();
            //info = pipelineInfo;
            if (pipelineInfo == null)
            {
                errMessage = "create default graphic pipeline information failed.";
                return false;
            }
            VkPrimitiveTopology primitive;
            foreach (int i in Enum.GetValues(typeof(VkPrimitiveTopology)))
            {
                primitive = (VkPrimitiveTopology)i;                
                SetPrimitive(primitive);
                if (primitive == VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST ||
                    primitive == VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_TRIANGLE_STRIP ||
                    primitive == VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_TRIANGLE_FAN ||
                    primitive == VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST_WITH_ADJACENCY ||
                    primitive == VkPrimitiveTopology.VK_PRIMITIVE_TOPOLOGY_TRIANGLE_STRIP_WITH_ADJACENCY)
                {
                    //三角形填充模式
                    SetPolygonMode(VkPolygonMode.VK_POLYGON_MODE_FILL);
                    if (!CreateGraphicsPipeLine(pipelineInfo))
                        return false;

                    //三角形填充模式-关闭深度写入
                    SetDepthWriteEnable(false);
                    if (!CreateGraphicsPipeLine(pipelineInfo))return false;
                    SetDepthWriteEnable(true);

                    //Validation: [ UNASSIGNED-GeneralParameterError-DeviceFeature ] Object: VK_NULL_HANDLE (Type = 0) | 
                    //vkCreateGraphicsPipelines parameter, VkPolygonMode pCreateInfos->pRasterizationState->polygonMode 
                    //cannot be VK_POLYGON_MODE_POINT or VK_POLYGON_MODE_LINE if VkPhysicalDeviceFeatures->fillModeNonSolid is false.
                    // not supported feature??
                    SetPolygonMode(VkPolygonMode.VK_POLYGON_MODE_LINE);
                    if (!CreateGraphicsPipeLine(pipelineInfo))
                        return false; 
                }  
                else
                {
                    SetPolygonMode(VkPolygonMode.VK_POLYGON_MODE_LINE);
                    if (!CreateGraphicsPipeLine(pipelineInfo))
                        return false;
                }
            }
            //create all the primitives
            return true;
        }
        public bool CreateGraphicsPipeLine(VkGraphicsPipelineCreateInfo info)
        {
            VkGraphicsPipelineCreateInfo createinfo;
            if (info == null)
            {
                CreateDefaultPipelineInfo();
                //info = pipelineInfo;
                if (pipelineInfo == null)
                {
                    errMessage = "create default graphic pipeline information failed.";
                    return false;
                }
            }

            createinfo = pipelineInfo;

            if (pipelineLayout == null)
            {
                if (!CreatePipelineLayout()) return false;
            }
            pipelineInfo.layout = pipelineLayout;
            //create pipe line - bug exist - easy to crash -jian 28 Feb 2018       
            //SetPolygonMode(VkPolygonMode.VK_POLYGON_MODE_FILL);
           
            //not supported feature POLYGON_MODEL_LINE   
            //VkPolygonMode pCreateInfos->pRasterizationState->polygonMode 
            //cannot be VK_POLYGON_MODE_POINT or VK_POLYGON_MODE_LINE 
            //if VkPhysicalDeviceFeatures->fillModeNonSolid is false.
            if (VulkanAPI.vkCreateGraphicsPipelines(device, null, 1, ref createinfo, out graphicsPipeline)
                     != SharpVulkan.VkResult.VK_SUCCESS)
            {
                errMessage = "failed to create graphics pipeline!";
                return false;
            }
            errMessage = "created graphics pipeline successfully!";

            VkGraphicsPipelineCreateInfo info1 = CopyPipelineInfo(pipelineInfo);

            //do not use : pipelineInfos.Add(info); 
            pipelineInfos.Add(info1);

            graphicsPipelines.Add(graphicsPipeline);

            return true;
        }
        public void EndShader()
        {
            //destroy shader modules
            if (vertShaderModule != null)
            {
                VulkanAPI.vkDestroyShaderModule(device, vertShaderModule);
            }
            if (fragShaderModule != null)
            {
                VulkanAPI.vkDestroyShaderModule(device, fragShaderModule);
            }
        }
        public void ClearTextureImageContext()
        {
            if (textureBitmap != null)
            { 
                textureBitmap.Dispose();
                textureBitmap = null;
            }
            //CVulkan.ReleaseSampler(device, ref textureSampler);
            CVulkan.ReleaseImage(device, ref textureImage);
            //CVulkan.ReleaseBufferMemory(device, ref textureImageMemory);            
            //CVulkan.ReleaseImageView(device, ref textureImageView);
            
            textureSampler = null;
            textureImageMemory = null;
            textureImageView = null;
            //textureImage = null;
        }
        public void Clearup()
        {
            if (device != null)
            {
                VulkanAPI.vkDeviceWaitIdle(device);
            }

            cleanupSwapChain();
            CleanGraphicsPipelines();

            if (textureSampler != null)
            {
                VulkanAPI.vkDestroySampler(device, textureSampler);
                textureSampler = null;
            }
            if (textureImageView != null)
            {
                VulkanAPI.vkDestroyImageView(device, textureImageView);
                textureImageView = null;
            }
            if (textureImage != null)
            {
                VulkanAPI.vkDestroyImage(device, textureImage);
                textureImage = null;
            }
            if (textureImageMemory != null)
            {
                VulkanAPI.vkFreeMemory(device, textureImageMemory);
                textureImageMemory = null;
            }
            if (descriptorSet != null)
            {
                VulkanAPI.vkFreeDescriptorSets(device, descriptorPool, new[] { descriptorSet });
                descriptorSet = null;
            }
            if (descriptorPool != null)
            {
                VulkanAPI.vkDestroyDescriptorPool(device, descriptorPool);
                descriptorPool = null;
            }
            if (descriptorSetLayout != null)
            {
                VulkanAPI.vkDestroyDescriptorSetLayout(device, descriptorSetLayout);
                descriptorSetLayout = null;
            }
            ClearDrawBuffer();

            if (uniformBuffer != null)
            {
                VulkanAPI.vkDestroyBuffer(device, uniformBuffer);
                uniformBuffer = null;
            }
            if (uniformBufferMemory != null)
            {
                VulkanAPI.vkFreeMemory(device, uniformBufferMemory);
                uniformBufferMemory = null;
            }
            if (indexBuffer != null)
            {
                VulkanAPI.vkDestroyBuffer(device, indexBuffer);
                indexBuffer = null;
            }
            if (indexBufferMemory != null)
            {
                VulkanAPI.vkFreeMemory(device, indexBufferMemory);
                indexBufferMemory = null;
            }
            if (vertexBuffer != null)
            {
                VulkanAPI.vkDestroyBuffer(device, vertexBuffer);
                vertexBuffer = null;
            }
            if (vertexBufferMemory != null)
            {
                VulkanAPI.vkFreeMemory(device, vertexBufferMemory);
                vertexBufferMemory = null;
            }

            if (renderFinishedSemaphore != null)
            {
                VulkanAPI.vkDestroySemaphore(device, renderFinishedSemaphore);
                renderFinishedSemaphore = null;
            }
            if (imageAvailableSemaphore != null)
            {
                VulkanAPI.vkDestroySemaphore(device, imageAvailableSemaphore);
                imageAvailableSemaphore = null;
            }
            if (commandPool != null)
            {
                VulkanAPI.vkDestroyCommandPool(device, commandPool);
                commandPool = null;
            }
            if (device != null)
            {
                VulkanAPI.vkDestroyDevice(device);
                device = null;
            }
            //DestroyDebugReportCallbackEXT(instance, callback, nullptr);
            if (vulkanSurface != null)
            {
                VulkanAPI.vkDestroySurfaceKHR(vkInstance, vulkanSurface);
                vulkanSurface = null;
            }
            if (vkInstance != null)
            {
                VulkanAPI.vkDestroyInstance(vkInstance);
                vkInstance = null;
            }
            if (glfwWindow != null) glfw3.Glfw.DestroyWindow(glfwWindow);
            glfw3.Glfw.Terminate();
        }
    }
}